const express = require('express');
const router = express.Router();
const { createNewUser, loginUser, logoutUser, getUserList, getUserScores, getLevelScores } = require('./controller');

/**
 * @swagger
 * /api/user:
 *   post:
 *     summary: Registrar un nou usuari
 *     tags: [Users]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *              $ref: '#/components/schemas/User'
 *     responses:
 *       201:
 *         description: Usuari registrat correctament
 *         content:
 *           { api_key }
 *       400:
 *         $ref: '#/components/responses/BadRequest'
 *       401:
 *         $ref: '#/components/responses/Unauthorized'
 *       500:
 *         $ref: '#/components/responses/ServerError'
 */
router.post('/user', createNewUser);

/**
 * @swagger
 */
router.post('/user/login', loginUser);
/**
 * @swagger
 */
router.post('/user/logout', logoutUser);
/**
 * @swagger
 */
router.get('/user/list', getUserList);
/**
 * @swagger
 */
router.get('/score/user/:id', getUserScores);
/**
 * @swagger
 */
router.get('/score/level/:id', getLevelScores);

module.exports = router;