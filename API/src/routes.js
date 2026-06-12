const { Router } = require("express");
const {
  getSongs,    getSongById,   addSong,
  getPlayers,  getPlayerById, addPlayer,
  submitScore, getScoresBySong,
} = require("./controller");

const router = Router();

// ── Songs ─────────────────────────────────────────────────────────────────────
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
router.get("/songs",          getSongs);

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
 *           type: uuid
 *         example: 019ebbb2-31cb-7879-a4a6-9aa3e6b89ff3
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
router.get("/songs/:songId",  getSongById);

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
 *               songId:    { type: uuid, example: 019ebbb2-31cb-7879-a4a6-9aa3e6b89ff3 }
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
router.post("/songs",         addSong);

// ── Players ───────────────────────────────────────────────────────────────────

/**
 * @openapi
 * /players:
 *   get:
 *     summary: Llista tots els jugadors
 *     description: Retorna una llista de tots els jugadors registrats al sistema, sense incloure les seves puntuacions
 *     tags:
 *       - Players
 *     responses:
 *       200:
 *         description: Llista de jugadors obtinguda correctament
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 players:
 *                   type: array
 *                   items:
 *                     type: object
 *                     properties:
 *                       playerId:
 *                         type: string
 *                         format: uuid
 *                         example: "019ebbb2-31cb-7a60-8e1d-ce9956756667"
 *                         description: ID únic del jugador
 *                       playerName:
 *                         type: string
 *                         example: "RhythmMaster"
 *                         description: Nom del jugador
 *       500:
 *         description: Error intern del servidor
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 *             example:
 *               error: "Error en obtenir els jugadors"
 */
router.get("/players", getPlayers);

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
 *           type: uuid
 *         example: 019ebbb2-31cb-7a60-8e1d-ce9956756667
 *     responses:
 *       200:
 *         description: Dades del jugador i puntuacions agrupades per cançó (fins a 5 puntuacions per cançó)
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
router.get("/players/:playerId",        getPlayerById);

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
 *             required: [playerName]
 *             properties:
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
router.post("/players",                 addPlayer);

// ── Scores ────────────────────────────────────────────────────────────────────
/**
 * @openapi
 * /scores/{playerId}:
 *   post:
 *     summary: Envia una nova puntuació d'un jugador en una cançó
 *     description: >
 *       Afegeix la puntuació a l'historial d'aquesta cançó pel jugador.
 *       Es conserven únicament les **5 millors puntuacions** ordenades per highscore descendent
 *       Si el jugador ja té 5 puntuacions i la nova no sepra la pitjor, es descarta.
 *       Si el jugador ya tiene 5 scores y el nuevo no supera al peor, se descarta.
 *     tags: [Players]
 *     parameters:
 *       - in: path
 *         name: playerId
 *         required: true
 *         schema:
 *           type: uuid
 *         example: 019ebbb2-31cb-7a60-8e1d-ce9956756667
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [playerId, highscore, maxCombo, rank]
 *             properties:
 *               playerId:  { type: uuid,    example: 019ebbb2-31cb-7a60-8e1d-ce9956756667}
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
router.post("/scores/:playerId",  submitScore);

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
 *           type: uuid
 *         example: 019ebbb2-31cb-7879-a4a6-9aa3e6b89ff3
 *     responses:
 *       200:
 *         description: Llista de puntuacions ordenada
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 songId:
 *                   type: uuid
 *                   example: 019ebbb2-31cb-7879-a4a6-9aa3e6b89ff3
 *                 scores:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/LeaderboardEntry'
 *       404:
 *         description: Cançó no trobada
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/Error'
 */
router.get("/scores/:songId", getScoresBySong);

module.exports = router;
