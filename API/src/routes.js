const express = require('express');
const router = express.Router();


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
app.get("/songs", listSongs);

