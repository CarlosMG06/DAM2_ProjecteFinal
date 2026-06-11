const express = require("express");
const fs = require("fs");
const path = require("path");
const swaggerUi = require("swagger-ui-express");
const swaggerJsdoc = require("swagger-jsdoc");

const app = express();
const PORT = 3000;
const DB_PATH = path.join(__dirname, "db.json");

// ── Middleware ────────────────────────────────────────────────────────────────
app.use(express.json());

// ── Swagger ───────────────────────────────────────────────────────────────────
const swaggerSpec = swaggerJsdoc({
  definition: {
    openapi: "3.0.0",
    info: {
      title: "Rhythm Game API",
      version: "1.0.0",
      description:
        "API para gestionar canciones, jugadores y puntuaciones de un juego de ritmo. " +
        "Los datos se persisten en un archivo `db.json`.",
    },
    components: {
      schemas: {
        ChartNote: {
          type: "object",
          properties: {
            inputBeat: { type: "number", example: 1.5, description: "Beat en el que aparece la nota" },
            inputKey:  { type: "string", enum: ["left", "right"], example: "left" },
          },
        },
        Song: {
          type: "object",
          properties: {
            songId:    { type: "string",  example: "s001" },
            songTitle: { type: "string",  example: "Neon Pulse" },
            bpm:       { type: "number",  example: 140 },
            audioFile: { type: "string",  example: "neon_pulse.mp3" },
            offset:    { type: "number",  example: 0.05, description: "Offset en segundos" },
            chart:     { type: "array", items: { $ref: "#/components/schemas/ChartNote" } },
          },
        },
        Run: {
          type: "object",
          properties: {
            highscore: { type: "integer", example: 98500 },
            maxCombo:  { type: "integer", example: 312 },
            rank:      { type: "string",  example: "S" },
          },
        },
        LevelEntry: {
          type: "object",
          properties: {
            songId: { type: "string", example: "s001" },
            runs: {
              type: "array",
              maxItems: 5,
              description: "Hasta 5 mejores runs, ordenados por highscore descendente",
              items: { $ref: "#/components/schemas/Run" },
            },
          },
        },
        Player: {
          type: "object",
          properties: {
            playerId:   { type: "string", example: "p001" },
            playerName: { type: "string", example: "RhythmMaster" },
            levelRuns: {
              type: "array",
              items: { $ref: "#/components/schemas/LevelEntry" },
            },
          },
        },
        SongScore: {
          type: "object",
          description: "Mejor puntuación de un jugador en una canción concreta",
          properties: {
            playerId:   { type: "string",  example: "p001" },
            playerName: { type: "string",  example: "RhythmMaster" },
            highscore:  { type: "integer", example: 98500 },
            maxCombo:   { type: "integer", example: 312 },
            rank:       { type: "string",  example: "S" },
          },
        },
        Error: {
          type: "object",
          properties: {
            error: { type: "string", example: "Song not found" },
          },
        },
      },
    },
  },
  apis: [__filename],
});

app.use("/docs", swaggerUi.serve, swaggerUi.setup(swaggerSpec));

// ── Helpers ───────────────────────────────────────────────────────────────────
function readDB() {
  return JSON.parse(fs.readFileSync(DB_PATH, "utf-8"));
}
function writeDB(data) {
  fs.writeFileSync(DB_PATH, JSON.stringify(data, null, 2), "utf-8");
}

// ── SONGS ─────────────────────────────────────────────────────────────────────

/**
 * @openapi
 * /songs:
 *   get:
 *     summary: Lista todas las canciones
 *     tags: [Songs]
 *     responses:
 *       200:
 *         description: Array con todas las canciones
 *         content:
 *           application/json:
 *             schema:
 *               type: array
 *               items:
 *                 $ref: '#/components/schemas/Song'
 */
app.get("/songs", (req, res) => {
  const db = readDB();
  res.json(db.songs);
});

/**
 * @openapi
 * /songs/{songId}:
 *   get:
 *     summary: Obtiene una canción por ID
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
 *         description: Datos de la canción
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Song'
 *       404:
 *         description: Canción no encontrada
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
app.get("/songs/:songId", (req, res) => {
  const db = readDB();
  const song = db.songs.find((s) => s.songId === req.params.songId);
  if (!song) return res.status(404).json({ error: "Song not found" });
  res.json(song);
});

/**
 * @openapi
 * /songs:
 *   post:
 *     summary: Añade una nueva canción
 *     tags: [Songs]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [songId, songTitle, bpm, audioFile]
 *             properties:
 *               songId:    { type: string, example: s003 }
 *               songTitle: { type: string, example: Dark Groove }
 *               bpm:       { type: number, example: 120 }
 *               audioFile: { type: string, example: dark_groove.mp3 }
 *               offset:    { type: number, example: 0.0 }
 *               chart:
 *                 type: array
 *                 items:
 *                   $ref: '#/components/schemas/ChartNote'
 *     responses:
 *       201:
 *         description: Canción creada
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Song'
 *       400:
 *         description: Faltan campos obligatorios
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 *       409:
 *         description: Ya existe una canción con ese songId
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
app.post("/songs", (req, res) => {
  const db = readDB();
  const { songId, songTitle, bpm, audioFile, offset, chart } = req.body;

  if (!songId || !songTitle || !bpm || !audioFile) {
    return res.status(400).json({ error: "songId, songTitle, bpm and audioFile are required" });
  }
  if (db.songs.find((s) => s.songId === songId)) {
    return res.status(409).json({ error: "A song with that songId already exists" });
  }

  const newSong = { songId, songTitle, bpm, audioFile, offset: offset ?? 0, chart: chart ?? [] };
  db.songs.push(newSong);
  writeDB(db);
  res.status(201).json(newSong);
});

// ── PLAYERS ───────────────────────────────────────────────────────────────────

/**
 * @openapi
 * /players/{playerId}:
 *   get:
 *     summary: Obtiene un jugador con todos sus runs
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
 *         description: Datos del jugador y su historial de runs
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Player'
 *       404:
 *         description: Jugador no encontrado
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
app.get("/players/:playerId", (req, res) => {
  const db = readDB();
  const player = db.players.find((p) => p.playerId === req.params.playerId);
  if (!player) return res.status(404).json({ error: "Player not found" });
  res.json(player);
});

/**
 * @openapi
 * /players:
 *   post:
 *     summary: Registra un nuevo jugador
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
 *         description: Jugador creado
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Player'
 *       400:
 *         description: Faltan campos obligatorios
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 *       409:
 *         description: Ya existe un jugador con ese playerId
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
app.post("/players", (req, res) => {
  const db = readDB();
  const { playerId, playerName } = req.body;

  if (!playerId || !playerName) {
    return res.status(400).json({ error: "playerId and playerName are required" });
  }
  if (db.players.find((p) => p.playerId === playerId)) {
    return res.status(409).json({ error: "A player with that playerId already exists" });
  }

  const newPlayer = { playerId, playerName, levelRuns: [] };
  db.players.push(newPlayer);
  writeDB(db);
  res.status(201).json(newPlayer);
});

/**
 * @openapi
 * /players/{playerId}/runs:
 *   post:
 *     summary: Envía una nueva puntuación para un jugador
 *     description: >
 *       Añade el run al historial de esa canción para el jugador.
 *       Se conservan únicamente los **5 mejores runs** ordenados por highscore descendente.
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
 *         description: Jugador actualizado con el nuevo historial de runs
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Player'
 *       400:
 *         description: Faltan campos obligatorios
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 *       404:
 *         description: Jugador o canción no encontrados
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
app.post("/players/:playerId/runs", (req, res) => {
  const MAX_RUNS_PER_SONG = 5;

  const db = readDB();
  const player = db.players.find((p) => p.playerId === req.params.playerId);
  if (!player) return res.status(404).json({ error: "Player not found" });

  const { songId, highscore, maxCombo, rank } = req.body;
  if (!songId || highscore == null || maxCombo == null || !rank) {
    return res.status(400).json({ error: "songId, highscore, maxCombo and rank are required" });
  }
  if (!db.songs.find((s) => s.songId === songId)) {
    return res.status(404).json({ error: "Song not found" });
  }

  let entry = player.levelRuns.find((e) => e.songId === songId);
  if (!entry) {
    entry = { songId, runs: [] };
    player.levelRuns.push(entry);
  }

  entry.runs.push({ highscore, maxCombo, rank });
  entry.runs.sort((a, b) => b.highscore - a.highscore);
  entry.runs = entry.runs.slice(0, MAX_RUNS_PER_SONG);

  writeDB(db);
  res.json(player);
});

// ── SCORES BY SONG ────────────────────────────────────────────────────────────

/**
 * @openapi
 * /scores/{songId}:
 *   get:
 *     summary: Puntuaciones de todos los jugadores en una canción
 *     description: Devuelve el mejor run de cada jugador para esa canción, ordenados de mayor a menor highscore.
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
 *         description: Lista de puntuaciones ordenada
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
 *         description: Canción no encontrada
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
app.get("/scores/:songId", (req, res) => {
  const db = readDB();
  const { songId } = req.params;

  if (!db.songs.find((s) => s.songId === songId)) {
    return res.status(404).json({ error: "Song not found" });
  }

  const scores = db.players
    .filter((p) => p.levelRuns.some((e) => e.songId === songId))
    .map((p) => {
      const entry = p.levelRuns.find((e) => e.songId === songId);
      const best = entry.runs[0];
      return {
        playerId: p.playerId,
        playerName: p.playerName,
        highscore: best.highscore,
        maxCombo: best.maxCombo,
        rank: best.rank,
      };
    })
    .sort((a, b) => b.highscore - a.highscore);

  res.json({ songId, scores });
});

// ── 404 catch-all ─────────────────────────────────────────────────────────────
app.use((req, res) => {
  res.status(404).json({ error: "Endpoint not found" });
});

// ── Start ─────────────────────────────────────────────────────────────────────
app.listen(PORT, () => {
  console.log(`Rhythm API running on http://localhost:${PORT}`);
  console.log(`Swagger docs available at http://localhost:${PORT}/docs`);
});

module.exports = app;
