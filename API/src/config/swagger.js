const swaggerJsDoc = require('swagger-jsdoc');

const swaggerOptions = {
  definition: {
    openapi: "3.0.0",
    info: {
      title: "Fruity Tunes API",
      version: "1.0.0",
      description:
        "API per gestionar cançons, jugadors i puntuacions del joc de ritme Fruity Tunes"
    },
    components: {
      schemas: {
        ChartNote: {
          type: "object",
          properties: {
            inputBeat: { type: "number", example: 1.5, description: "Beat on apareix la nota" },
            inputKey:  { type: "string", enum: ["left", "right"], example: "left" },
          },
        },
        Song: {
          type: "object",
          properties: {
            songId:    { type: "uuid",    example: "019ebbb2-31cb-7879-a4a6-9aa3e6b89ff3" },
            songTitle: { type: "string",  example: "Must, Be Nice" },
            bpm:       { type: "number",  example: 140 },
            audioFile: { type: "string",  example: "grapes1_must-be-nice.mp3" },
            offset:    { type: "number",  example: 0.05, description: "Offset en segons" },
            chart:     { type: "array", items: { $ref: "#/components/schemas/ChartNote" } },
          },
        },
        Score: {
          type: "object",
          properties: {
            scoreId:   { type: "uuid",    example: "019ebbb2-31cb-7250-925d-c5f8341fc4c2"},
            highscore: { type: "integer", example: 98500 },
            maxCombo:  { type: "integer", example: 312 },
            rank:      { type: "string",  example: "S" },
          },
        },
        LevelEntry: {
          type: "object",
          properties: {
            songId: { type: "uuid", example: "019ebbb2-31cb-7879-a4a6-9aa3e6b89ff3" },
            scores: {
              type: "array",
              maxItems: 5,
              description: "Fins a 5 millors puntuacions, en ordre descendent",
              items: { $ref: "#/components/schemas/Score" },
            },
          },
        },
        Player: {
          type: "object",
          properties: {
            playerId:   { type: "uuid", example: "019ebbb2-31cb-7a60-8e1d-ce9956756667" },
            playerName: { type: "string", example: "RhythmMaster" },
            playerIcon: { type: "string", example: "profile_icon.png"},
            levelScores: {
              type: "array",
              items: { $ref: "#/components/schemas/LevelEntry" },
            },
          },
        },
        LeaderboardEntry: {
          type: "object",
          description: "Fila de la taula de classificacions",
          properties: {
            playerId:   { type: "uuid",  example: "019ebbb2-31cb-7a60-8e1d-ce9956756667" },
            playerName: { type: "string",  example: "RhythmMaster" },
            highscore:  { type: "integer", example: 98500 },
            maxCombo:   { type: "integer", example: 312 },
            rank:       { type: "string",  example: "S" },
          },
        },
        Error: {
          type: "object",
          properties: {
            error: { type: "string", example: "Cançó no trobada" },
          },
        },
      },
    },
  },
  apis: ['./src/routes.js'],
};

module.exports = swaggerJsDoc(swaggerOptions);
