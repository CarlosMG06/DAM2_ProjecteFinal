const { Op } = require("sequelize");
const { Song, ChartNote, Player, Score } = require("./config/database");
const { saveIcon } = require("./upload");
const path = require("path");

const ICON_URL_PREFIX  = "/static/icons"; // express.static en index.js

const MAX_SCORES_PER_SONG = 5;

// Auxiliar: construir la URL pública d'una icona segons el nom del fitxer
function iconUrl(req, filename) {
  return `${req.protocol}://${req.get("host")}${ICON_URL_PREFIX}/${filename}`;
}

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
  const { songTitle, bpm, audioFile, offset, chart } = req.body;
  if (!songTitle || !bpm || !audioFile) {
    return res.status(400).json({ error: "songTitle, bpm and audioFile are required" });
  }
  const song = await Song.create(
    { songTitle, bpm, audioFile, offset: offset ?? 0, chart: chart ?? [] },
    { include: [{ model: ChartNote, as: "chart" }] }
  );
  res.status(201).json(song);
}

// ── PLAYERS ───────────────────────────────────────────────────────────────────

// GET /players - Llista el nom i la icona de tots els jugadors
async function getPlayers(req, res) {
  const players = await Player.findAll({
    attributes: ["playerId", "playerName", "playerIcon"],
    order: [["playerName", "ASC"]],
  });

  const result = players.map((p) => ({
    playerId:   p.playerId,
    playerName: p.playerName,
    playerIcon: p.playerIcon ? iconUrl(req, p.playerIcon) : null,
  }));

  res.status(200).json({ players: result });
}

// GET /players/:playerId - Obté un jugador amb totes les seves puntuacions
async function getPlayerById(req, res) {
  const player = await Player.findByPk(req.params.playerId, {
    include: [{ model: Score, as: "scores", include: [Song] }],
  });
  if (!player) return res.status(404).json({ error: "Player not found" });

  const scoresMap = {};
  for (const score of player.scores) {
    const sid = score.songId;
    if (!scoresMap[sid]) scoresMap[sid] = [];
    scoresMap[sid].push({ highscore: score.highscore, maxCombo: score.maxCombo, rank: score.rank });
  }
  const levelScores = Object.entries(scoresMap).map(([songId, scores]) => ({ songId, scores }));

  res.json({
    playerId:   player.playerId,
    playerName: player.playerName,
    playerIcon: player.playerIcon ? iconUrl(req, player.playerIcon) : null,
    levelScores,
  });
}

// POST /players - Registra un nou jugador
// Camps: playerName (text), file (fitxer, opcional)
async function addPlayer(req, res) {
  const { playerName } = req.body;

  if (!playerName) {
    return res.status(400).json({ error: "playerName is required" });
  }

  // Generar UUID
  const player = await Player.create({ playerName });
  const { playerId } = player;

  // Donar nom al fitxer (la UUID + .extensió)
  if (req.file) {
    const iconFilename = saveIcon(playerId, req.file.buffer, req.file.mimetype)
  }

  // Desar nom dins la BD
  await player.update({ playerIcon: iconFilename });

  res.status(201).json({
    ...player.toJSON(),
    playerIcon: iconUrl(req, iconFilename),
    levelScores: [],
  });
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

  await Score.create({ playerId, songId, highscore, maxCombo, rank });

  const allScores = await Score.findAll({
    where: { playerId, songId },
    order: [["highscore", "DESC"]],
  });
  if (allScores.length > MAX_SCORES_PER_SONG) {
    const toDelete = allScores.slice(MAX_SCORES_PER_SONG).map((r) => r.id);
    await Score.destroy({ where: { id: { [Op.in]: toDelete } } });
  }

  return getPlayerById(req, res);
}

// GET /scores/:songId - Llista puntuacions de tots els jugadors en una cançó
async function getScoresBySong(req, res) {
  const { songId } = req.params;

  const song = await Song.findByPk(songId);
  if (!song) return res.status(404).json({ error: "Song not found" });

  const scores = await Score.findAll({
    where: { songId },
    include: [{ model: Player, attributes: ["playerId", "playerName", "playerIcon"] }],
    order: [["highscore", "DESC"]],
  });

  const seen = new Set();
  const leaderboardEntries = [];
  for (const score of scores) {
    if (seen.has(score.playerId)) continue;
    seen.add(score.playerId);
    leaderboardEntries.push({
      playerId:   score.Player.playerId,
      playerName: score.Player.playerName,
      playerIcon: score.Player.playerIcon ? iconUrl(req, score.Player.playerIcon) : null,
      highscore:  score.highscore,
      maxCombo:   score.maxCombo,
      rank:       score.rank,
    });
  }

  res.json({ songId, leaderboardEntries });
}

module.exports = { getSongs, getSongById, addSong, getPlayers, getPlayerById, addPlayer, submitScore, getScoresBySong };
