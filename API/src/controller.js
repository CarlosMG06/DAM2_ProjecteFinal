const { validateUUID } = require('./utils');
const { Op } = require("sequelize");
const { Song, ChartNote, Player, Score: Score } = require("./database");

const MAX_SCORES_PER_SONG = 5;

// ── SONGS ─────────────────────────────────────────────────────────────────────

// GET /songs - Llista totes les cançons
async function getSongs(req, res) {
  const songs = await Song.findAll({ include: [{ model: ChartNote, as: "chart" }] });
  
  res.json(songs);
}

// GET /songs/:songId - Obté una cançó per ID
async function getSongById(req, res) {
  const song = await Song.findByPk(req.params.songId, {
    include: [{ model: ChartNote, as: "chart" }],
  });
  
  if (!song) return res.status(404).json({ error: "Song not found" });
  
  res.json(song);
}

// POST /songs - Afegeix una nova cançó
async function addSong(req, res) {
  const {songTitle, bpm, audioFile, offset, chart } = req.body;

  if (!songTitle || !bpm || !audioFile) {
    return res.status(400).json({ error: "songId, songTitle, bpm and audioFile are required" });
  }

  const song = await Song.create(
    { songTitle, bpm, audioFile, offset: offset ?? 0, chart: chart ?? [] },
    { include: [{ model: ChartNote, as: "chart" }] }
  );
  res.status(201).json(song);
}

// ── PLAYERS ───────────────────────────────────────────────────────────────────

// GET /players - Llista tots els jugadors (sense puntuacions)
async function getPlayers(req, res) {
  const players = await Player.findAll({
    attributes: ['playerId', 'playerName'],
    order: [['playerName', 'ASC']]
  });

  res.status(200).json({
    players: players
  });
}

// GET /players/:playerId - Obté un jugador amb totes les seves puntuacions
async function getPlayerById(req, res) {
  const player = await Player.findByPk(req.params.playerId, {
    include: [{ model: Score, as: "scores", include: [Song] }],
  });
  if (!player) return res.status(404).json({ error: "Player not found" });

  // Agrupa puntuacions per cançó 
  const scoresMap = {};
  for (const score of player.scores) {
    const sid = score.songId;
    if (!scoresMap[sid]) scoresMap[sid] = [];
    scoresMap[sid].push({ highscore: score.highscore, maxCombo: score.maxCombo, rank: score.rank });
  }
  const levelScores = Object.entries(scoresMap).map(([songId, scores]) => ({ songId, scores }));

  res.json({ playerId: player.playerId, playerName: player.playerName, levelScores });
}

// POST /players - Registra un nou jugador
async function addPlayer(req, res) {
  const { playerName } = req.body;

  if (!playerName) {
    return res.status(400).json({ error: "playerNameis required" });
  }

  const player = await Player.create({ playerName });
  res.status(201).json({ ...player.toJSON(), levelScores: [] });
}

// ── SCORES ────────────────────────────────────────────────────────────────────

// POST /scores/:playerId - Envia una nova puntuació d'un jugador en una cançó
async function submitScore(req, res) {
  const { playerId } = req.params;
  const { songId, highscore, maxCombo, rank } = req.body;

  if (!songId || highscore == null || maxCombo == null || !rank) {
    return res.status(400).json({ error: "songId, highscore, maxCombo and rank are required" });
  }

  const [player, song] = await Promise.all([
    Player.findByPk(playerId),
    Song.findByPk(songId),
  ]);
  if (!player) return res.status(404).json({ error: "Player not found" });
  if (!song)   return res.status(404).json({ error: "Song not found" });

  // Insertar nova puntuació
  await Score.create({ playerId, songId, highscore, maxCombo, rank });

  // Desar només les millors MAX_SCORES_PER_SONG per aquesta combinació de jugador i cançó
  const allScores = await Score.findAll({
    where: { playerId, songId },
    order: [["highscore", "DESC"]],
  });
  if (allScores.length > MAX_SCORES_PER_SONG) {
    const toDelete = allScores.slice(MAX_SCORES_PER_SONG).map((r) => r.id);
    await Score.destroy({ where: { id: { [Op.in]: toDelete } } });
  }

  // Retornar la resposta completa del jugador
  return getPlayerById(req, res);
}

// GET /scores/:songId - Llista puntuacions de tots els jugadors en una cançó
async function getScoresBySong(req, res) {
  const { songId } = req.params;

  const song = await Song.findByPk(songId);
  if (!song) return res.status(404).json({ error: "Song not found" });

  // Puntuacions de jugadors ordenades de major a menor
  const scores = await Score.findAll({
    where: { songId },
    include: [{ model: Player, attributes: ["playerId", "playerName"] }],
    order: [["highscore", "DESC"]],
  });

  // Agafar només la millor puntuació de cada jugador
  const seen = new Set();
  const leaderboardEntries = [];
  for (const score of scores) {
    if (seen.has(score.playerId)) continue;
    seen.add(score.playerId);
    leaderboardEntries.push({
      playerId:   score.Player.playerId,
      playerName: score.Player.playerName,
      highscore:  score.highscore,
      maxCombo:   score.maxCombo,
      rank:       score.rank,
    });
  }

  res.json({ songId, leaderboardEntries });
}

module.exports = { getSongs, getSongById, addSong, getPlayers, getPlayerById, addPlayer, submitScore, getScoresBySong };