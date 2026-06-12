const { Sequelize, DataTypes } = require("sequelize");
const { v7: uuidv7 } = require("uuid");

// Credentials

const sequelize = new Sequelize(
  process.env.DB_NAME,
  process.env.DB_USER,
  process.env.DB_PASSWORD,
  {
    host:    process.env.DB_HOST,
    port:    process.env.DB_PORT,
    dialect: "mysql",
    logging: false, // Set to console.log to see SQL queries
  }
);

// Models

// ── Song ──────────────────────────────────────────────────────────────────────
const Song = sequelize.define("Song", {
  songId: {
    type:         DataTypes.UUID,
    primaryKey:   true,
    allowNull:    false,
    defaultValue: uuidv7,
  },
  songTitle: {
    type:      DataTypes.STRING,
    allowNull: false,
  },
  bpm: {
    type:      DataTypes.FLOAT,
    allowNull: false,
  },
  audioFile: {
    type:      DataTypes.STRING,
    allowNull: false,
  },
  offset: {
    type:         DataTypes.FLOAT,
    allowNull:    false,
    defaultValue: 0,
  },
});
 
// ── ChartNote ─────────────────────────────────────────────────────────────────
// A note belonging to a Song
const ChartNote = sequelize.define("ChartNote", {
  id: {
    type:         DataTypes.UUID,
    primaryKey:   true,
    allowNull:    false,
    defaultValue: uuidv7,
  },
  inputBeat: {
    type:      DataTypes.FLOAT,
    allowNull: false,
  },
  inputKey: {
    type:      DataTypes.ENUM("left", "right"),
    allowNull: false,
  },
});
 
// ── Player ────────────────────────────────────────────────────────────────────
const Player = sequelize.define("Player", {
  playerId: {
    type:         DataTypes.UUID,
    primaryKey:   true,
    allowNull:    false,
    defaultValue: uuidv7
  },
  playerName: {
    type:      DataTypes.STRING,
    allowNull: false,
  },
});
 
// ── Score ───────────────────────────────────────────────────────────────────────
// A score result by a Player on a Song.
// Only the top 5 per (playerId, songId) are kept (enforced in the controller).
const Score = sequelize.define("Score", {
  id: {
    type:          DataTypes.UUID,
    primaryKey:    true,
    allowNull:     false,
    defaultValue: uuidv7
  },
  highscore: {
    type:      DataTypes.INTEGER,
    allowNull: false,
  },
  maxCombo: {
    type:      DataTypes.INTEGER,
    allowNull: false,
  },
  rank: {
    type:      DataTypes.STRING(8),
    allowNull: false,
  },
});
 
// ── Associations ──────────────────────────────────────────────────────────────
Song.hasMany(ChartNote, { foreignKey: "songId", onDelete: "CASCADE", as: "chart" });
ChartNote.belongsTo(Song, { foreignKey: "songId" });
 
Player.hasMany(Score, { foreignKey: "playerId", onDelete: "CASCADE", as: "scores" });
Score.belongsTo(Player, { foreignKey: "playerId" });
 
Song.hasMany(Score, { foreignKey: "songId", onDelete: "CASCADE" });
Score.belongsTo(Song, { foreignKey: "songId" });


module.exports = { sequelize, Song, ChartNote, Player, Score };