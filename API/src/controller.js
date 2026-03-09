const { sequelize, User, Score, Level } = require('./database');
const { generateUUID, validateUUID } = require('./utils');

/**
 * Registra un nou usuari
 * @route POST /api/user
 */
const createNewUser = async (req, res) => {

}

/**
 * Inicia la sessió d'un usuari
 * @route POST /api/user/login
 */
const loginUser = async (req, res) => {

}

/**
 * Tanca la sessió d'un usuari
 * @route POST /api/user/logout
 */
const logoutUser = async (req, res) => {

}

/**
 * Retorna la llista dels usuaris registrats
 * @route GET /api/user/list
 */
const getUserList = async (req, res) => {

}

/**
 * Retorna la llista de puntuacions d'un usuari
 * @route GET /api/score/user/:id
 */
const getUserScores = async (req, res) => {

}

/**
 * Retorna la llista de puntuacions d'un nivell
 * @route GET /api/score/level/:id
 */
const getLevelScores = async (req, res) => {

}


// Exportació de les funcions públiques
module.exports = {
    createNewUser, loginUser, logoutUser, getUserList,
    getUserScores, getLevelScores
};