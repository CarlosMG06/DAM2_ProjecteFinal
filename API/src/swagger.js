const swaggerJsDoc = require('swagger-jsdoc');

const swaggerOptions = {
  definition: {
    openapi: "3.0.0",
    info: {
      title: "Rhythm Game API",
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
            songId:    { type: "string",  example: "s001" },
            songTitle: { type: "string",  example: "Must, Be Nice" },
            bpm:       { type: "number",  example: 140 },
            audioFile: { type: "string",  example: "grapes1_must-be-nice.mp3" },
            offset:    { type: "number",  example: 0.05, description: "Offset en segons" },
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
              description: "Fins a 5 millors puntuacions, en ordre descendent",
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
          description: "Millor puntuació d'un jugador en una cançó en concret",
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
};

module.exports = swaggerJsDoc(swaggerOptions);
