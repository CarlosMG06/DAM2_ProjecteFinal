const { validateUUID } = require('/middleware/validators');
const { Op } = require("sequelize");
const { Song, ChartNote, Player, Run } = require("./database");

const MAX_RUNS_PER_SONG = 5;

// ── SONGS ─────────────────────────────────────────────────────────────────────

/**
 * @openapi
 * /songs:
 *   get:
 *     summary: Llista totes les cançons
 *     tags: [Songs]
 *     responses:
 *       200:
 *         description: Array amb totes les cançons
 *         content:
 *           application/json:
 *             schema:
 *               type: array
 *               items:
 *                 $ref: '#/components/schemas/Song'
 */
async function getSongs(req, res) {
  const songs = await Song.findAll({ include: [{ model: ChartNote, as: "chart" }] });
  res.json(songs);
}

/**
 * @openapi
 * /songs/{songId}:
 *   get:
 *     summary: Obté una cançó per ID
 *     tags: [Songs]
 *     parameters:
 *       - in: path
 *         name: songId
 *         required: true
 *         schema:
 *           type: string
 *         example: s001
 *     responses:
 *       200:
 *         description: Dades de la cançó
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Song'
 *       404:
 *         description: Cançó no trobada
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
async function getSongById(req, res) {
  const song = await Song.findByPk(req.params.songId, {
    include: [{ model: ChartNote, as: "chart" }],
  });
  if (!song) return res.status(404).json({ error: "Song not found" });
  res.json(song);
}

/**
 * @openapi
 * /songs:
 *   post:
 *     summary: Afegeix una nova cançó
 *     tags: [Songs]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [songId, songTitle, bpm, audioFile]
 *             properties:
 *               songId:    { type: string, example: s001 }
 *               songTitle: { type: string, example: Must, Be Nice }
 *               bpm:       { type: number, example: 120 }
 *               audioFile: { type: string, example: grapes1_must-be-nice.mp3 }
 *               offset:    { type: number, example: 0.0 }
 *               chart:
 *                 type: array
 *                 items:
 *                   $ref: '#/components/schemas/ChartNote'
 *     responses:
 *       201:
 *         description: Cançó creada
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Song'
 *       400:
 *         description: Falten camps obligatoris
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 *       409:
 *         description: Ja existeix una cançó amb aquest songId
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
async function createSong(req, res) {
  const { songId, songTitle, bpm, audioFile, offset, chart } = req.body;

  if (!songId || !songTitle || !bpm || !audioFile) {
    return res.status(400).json({ error: "songId, songTitle, bpm and audioFile are required" });
  }
  const exists = await Song.findByPk(songId);
  if (exists) return res.status(409).json({ error: "A song with that songId already exists" });

  const song = await Song.create(
    { songId, songTitle, bpm, audioFile, offset: offset ?? 0, chart: chart ?? [] },
    { include: [{ model: ChartNote, as: "chart" }] }
  );
  res.status(201).json(song);
}

// ── PLAYERS ───────────────────────────────────────────────────────────────────

/**
 * @openapi
 * /players/{playerId}:
 *   get:
 *     summary: Obté un jugador amb totes les seves puntuacions
 *     tags: [Players]
 *     parameters:
 *       - in: path
 *         name: playerId
 *         required: true
 *         schema:
 *           type: string
 *         example: p001
 *     responses:
 *       200:
 *         description: Dades del jugador agrupades per cançó (fins a 5 puntuacions per cançó)
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Player'
 *       404:
 *         description: Jugador no trobat
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
async function getPlayerById(req, res) {
  const player = await Player.findByPk(req.params.playerId, {
    include: [{ model: Run, as: "runs", include: [Song] }],
  });
  if (!player) return res.status(404).json({ error: "Player not found" });

  // Agrupa puntuacions per cançó 
  const runsMap = {};
  for (const run of player.runs) {
    const sid = run.songId;
    if (!runsMap[sid]) runsMap[sid] = [];
    runsMap[sid].push({ highscore: run.highscore, maxCombo: run.maxCombo, rank: run.rank });
  }
  const levelRuns = Object.entries(runsMap).map(([songId, runs]) => ({ songId, runs }));

  res.json({ playerId: player.playerId, playerName: player.playerName, levelRuns });
}

/**
 * @openapi
 * /players:
 *   post:
 *     summary: Registra un nou jugador
 *     tags: [Players]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [playerId, playerName]
 *             properties:
 *               playerId:   { type: string, example: p003 }
 *               playerName: { type: string, example: ComboKing }
 *     responses:
 *       201:
 *         description: Jugador creat
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Player'
 *       400:
 *         description: Falten camps obligatoris
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 *       409:
 *         description: Ja existeix un jugador amb aquest playerId
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
async function createPlayer(req, res) {
  const { playerId, playerName } = req.body;

  if (!playerId || !playerName) {
    return res.status(400).json({ error: "playerId and playerName are required" });
  }
  const exists = await Player.findByPk(playerId);
  if (exists) return res.status(409).json({ error: "A player with that playerId already exists" });

  const player = await Player.create({ playerId, playerName });
  res.status(201).json({ ...player.toJSON(), levelRuns: [] });
}

/**
 * @openapi
 * /players/{playerId}/runs:
 *   post:
 *     summary: Envia una nova puntuació d'un jugador en una cançó
 *     description: >
 *       Afegeix la puntuació a l'historial d'aquesta cançó pel jugador.
 *       Es conserven únicament les **5 millors puntuacions** ordenades per highscore descendent
 *       Si el jugador ja té 5 puntuacions i la nova no sepra la pitjor, es descarta.
 *       Si el jugador ya tiene 5 runs y el nuevo no supera al peor, se descarta.
 *     tags: [Players]
 *     parameters:
 *       - in: path
 *         name: playerId
 *         required: true
 *         schema:
 *           type: string
 *         example: p001
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [songId, highscore, maxCombo, rank]
 *             properties:
 *               songId:    { type: string,  example: s001 }
 *               highscore: { type: integer, example: 105000 }
 *               maxCombo:  { type: integer, example: 400 }
 *               rank:      { type: string,  example: SS }
 *     responses:
 *       200:
 *         description: Jugador actualitzat amb el nou historial de puntuacions
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Player'
 *       400:
 *         description: Falten camps obligatoris
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 *       404:
 *         description: Jugador o cançó no trobats
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
async function submitRun(req, res) {
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
  await Run.create({ playerId, songId, highscore, maxCombo, rank });

  // Desar només les millors MAX_RUNS_PER_SONG per aquesta combinació de jugador i cançó
  const allRuns = await Run.findAll({
    where: { playerId, songId },
    order: [["highscore", "DESC"]],
  });
  if (allRuns.length > MAX_RUNS_PER_SONG) {
    const toDelete = allRuns.slice(MAX_RUNS_PER_SONG).map((r) => r.id);
    await Run.destroy({ where: { id: { [Op.in]: toDelete } } });
  }

  // Retornar la resposta completa del jugador
  return getPlayerById(req, res);
}

// ── SCORES ────────────────────────────────────────────────────────────────────

/**
 * @openapi
 * /scores/{songId}:
 *   get:
 *     summary: Puntuacions de tots els jugadors en una cançó
 *     description: Retorna la millor puntuació de cada jugador per aquesta cançó, ordenats de major a menor.
 *     tags: [Scores]
 *     parameters:
 *       - in: path
 *         name: songId
 *         required: true
 *         schema:
 *           type: string
 *         example: s001
 *     responses:
 *       200:
 *         description: Llista de puntuacions ordenada
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 songId:
 *                   type: string
 *                   example: s001
 *                 scores:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/SongScore'
 *       404:
 *         description: Cançó no trobada
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
async function getScoresBySong(req, res) {
  const { songId } = req.params;

  const song = await Song.findByPk(songId);
  if (!song) return res.status(404).json({ error: "Song not found" });

  // Puntuacions de jugadors ordenades de major a menor
  const runs = await Run.findAll({
    where: { songId },
    include: [{ model: Player, attributes: ["playerId", "playerName"] }],
    order: [["highscore", "DESC"]],
  });

  // Agafar només la millor puntuació de cada jugador
  const seen = new Set();
  const scores = [];
  for (const run of runs) {
    if (seen.has(run.playerId)) continue;
    seen.add(run.playerId);
    scores.push({
      playerId:   run.Player.playerId,
      playerName: run.Player.playerName,
      highscore:  run.highscore,
      maxCombo:   run.maxCombo,
      rank:       run.rank,
    });
  }

  res.json({ songId, scores });
}

module.exports = { getSongs, getSongById, createSong, getPlayerById, createPlayer, submitRun, getScoresBySong };