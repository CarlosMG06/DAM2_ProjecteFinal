/**
 * Insertació de dades per defecte
 * (Songs, ChartNotes, Players i Scores)
 */

const fs = require('fs');
const path = require('path');
const csvParser = require('csv-parser');
const { sequelize, Song, ChartNote, Player, Score } = require('../../src/config/database');
const { saveIcon } = require('../../src/upload');

const SONGS_DIR = path.join(__dirname, 'songs');
const DEFAULT_PLAYERS_FILE = path.join(__dirname, 'default_players.json');
const DEFAULT_SCORES_FILE = path.join(__dirname, 'default_scores.csv');
const ICONS_DIR = path.join(__dirname, "..", "uploads", "icons");

// Funció per llegir CSV
async function readCSV(filePath, requiredFields = []) {
  return new Promise((resolve, reject) => {
    const results = [];
    fs.createReadStream(filePath)
      .pipe(csvParser())
      .on('data', (data) => {
        const isValid = requiredFields.every(field => 
          data[field] !== undefined && data[field] !== ''
        );
        if (isValid) {
          results.push(data);
        }
      })
      .on('end', () => resolve(results))
      .on('error', reject);
  });
}

async function seedDatabase(force) {
  
  try {
    if (force) {
      console.log('!!! Mode force activat - Esborrant dades existents...');
      await sequelize.sync({ force: true });
      console.log('+++ Taules recreades');

      // Esborrar també qualsevol fitxer pujat
      fs.readdir(ICONS_DIR, (err, files) => {
        if (err) throw err;

        for (const file of files) {
          fs.unlink(path.join(ICONS_DIR, file), (err) => {
            if (err) throw err;
          });
        }
      });
    }
    

    if (!force) {
      // Verificar si ja hi ha dades
      const songCount = await Song.count();
      if (songCount > 0) {
        console.log(`!!! Ja existeixen ${songCount} cançons. Fes servir --force per sobrescriure.`);
        return;
      }
    }
    
    // ==================== 1. INSERTAR CANÇONS I CHARTS ====================
    console.log('\n>>> Insertant cançons i charts...');
    
    const songFolders = fs.readdirSync(SONGS_DIR).filter(item => {
      const itemPath = path.join(SONGS_DIR, item);
      return fs.statSync(itemPath).isDirectory();
    });
    
    const songsMap = new Map(); // songTitle -> songId
    
    for (const folder of songFolders) {
      const metadataPath = path.join(SONGS_DIR, folder, 'metadata.json');
      const chartPath = path.join(SONGS_DIR, folder, 'chart.csv');
      
      if (!fs.existsSync(metadataPath)) {
        console.log(`!!! ${folder}: metadata.json no trobat`);
        continue;
      }
      
      if (!fs.existsSync(chartPath)) {
        console.log(`!!! ${folder}: chart.csv no trobat`);
        continue;
      }
      
      // Leer metadata
      const metadata = JSON.parse(fs.readFileSync(metadataPath, 'utf8'));
      
      // Leer chart
      const chartNotes = await readCSV(chartPath, ['inputBeat', 'inputKey']);
      
      if (chartNotes.length === 0) {
        console.log(`!!! ${metadata.songTitle}: chart sense notes`);
        continue;
      }
      
      // Crear cançó
      const song = await Song.create({
        songTitle: metadata.songTitle,
        bpm: metadata.bpm,
        audioFile: metadata.audioFile,
        offset: metadata.offset || 0
      });
      
      // Insertar notes
      const notesToInsert = chartNotes.map(note => ({
        songId: song.songId,
        inputBeat: parseFloat(note.inputBeat),
        inputKey: note.inputKey.toLowerCase()
      }));
      
      await ChartNote.bulkCreate(notesToInsert);
      
      songsMap.set(metadata.songTitle, song.songId);
      console.log(`+++ ${metadata.songTitle} - ${chartNotes.length} notes`);
    }
    
    if (songsMap.size === 0) {
      console.log('!!! No s\'han trobat cançons vàlides');
      return;
    }
    
    // ==================== 2. INSERTAR JUGADORS PER DEFECTE ====================
    console.log('\n>>> Insertant jugadors per defecte...');
    
    if (!fs.existsSync(DEFAULT_PLAYERS_FILE)) {
      console.log('!!! default_players.json no trobat');
      return;
    }
    
    const playersData = JSON.parse(fs.readFileSync(DEFAULT_PLAYERS_FILE, 'utf8'));
    const playersMap = new Map(); // playerName -> playerId
    
    for (const playerName of playersData.players) {
      const player = await Player.create({
        playerName: playerName,
        isDefault: true
      });
      
      if (playerName === 'Freddy') {
          const buffer = fs.readFileSync(path.join(__dirname, 'freddy.png'));
          const iconFilename = saveIcon(player.playerId, buffer, 'image/png');
          await player.update({ playerIcon: iconFilename });
      }

      playersMap.set(playerName, player.playerId);
      console.log(`+++ ${playerName}`);
    }
    
    // ==================== 3. INSERTAR PUNTUACIONS PER DEFECTE ====================
    console.log('\n>>> Insertant puntuacions per defecte...');
    
    if (!fs.existsSync(DEFAULT_SCORES_FILE)) {
      console.log('!!!  default_scores.csv no trobat');
      return;
    }
    
    const scoresData = await readCSV(DEFAULT_SCORES_FILE, ['playerName', 'songTitle', 'highscore', 'maxCombo', 'rank']);
    
    let scoresInserted = 0;
    let scoresSkipped = 0;
    
    for (const scoreData of scoresData) {
      const playerId = playersMap.get(scoreData.playerName);
      const songId = songsMap.get(scoreData.songTitle);
      
      if (!playerId) {
        console.log(`!!! Jugador no trobat: ${scoreData.playerName}`);
        scoresSkipped++;
        continue;
      }
      
      if (!songId) {
        console.log(`!!! Cançó no trobada: ${scoreData.songTitle}`);
        scoresSkipped++;
        continue;
      }
      
      await Score.create({
        playerId: playerId,
        songId: songId,
        highscore: parseInt(scoreData.highscore),
        maxCombo: parseInt(scoreData.maxCombo),
        rank: scoreData.rank
      });
      
      scoresInserted++;
    }
    
    console.log(`+++ ${scoresInserted} puntuacions insertades`);
    if (scoresSkipped > 0) {
      console.log(`!!!  ${scoresSkipped} puntuacions omeses (jugador/cançó no trobat)`);
    }
    
    // ==================== RESUMEN FINAL ====================
    console.log('\n===== SEEDING COMPLETAT =====');
    
    const totalSongs = await Song.count();
    const totalNotes = await ChartNote.count();
    const totalPlayers = await Player.count();
    const totalScores = await Score.count();
    
    console.log(`\n📊 Resum final:`);
    console.log(`   🎵 Cançons: ${totalSongs}`);
    console.log(`   🎹 Notes totals: ${totalNotes}`);
    console.log(`   👥 Jugadors: ${totalPlayers}`);
    console.log(`   🏆 Puntuacions: ${totalScores}`);
    
  } catch (error) {
    console.error('❌ Error en fer seeding:', error);
    console.error(error.stack);
  } finally {
    // Tancar la connexió si es crida directament
    if (require.main === module) {
      await sequelize.close();
    }
  }
}

// Executar si es crida directament
if (require.main === module) {
  seedDatabase();
}

module.exports = { seedDatabase };