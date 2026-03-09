const swaggerJsDoc = require('swagger-jsdoc');

const swaggerOptions = {
    definition: {
        openapi: '3.0.0',
        info: {
            title: 'Fruity Tunes API',
            version: '1.0.0',
            description: 'API per accedir a la base de dades del joc'
        },
        servers: [
            {
                url: 'http://127.0.0.1:3000',
                description: 'Servidor de desenvolupament'
            }
        ],
        components: {
            schemas: {
                User: {
                    type: 'object',
                    properties: {
                       userId:  {
                            type: 'string',
                            format: 'uuid',
                       },
                       name: {
                            type: 'string'
                       },
                       password: {
                            type: 'string'
                       },
                       scores: {
                            type: 'array',
                            items: {
                                $ref: '#/components/schemas/Score'
                            }
                       }
                    }
                },
                Score: {
                    type: 'object',
                    properties: {
                        scoreId: {
                            type: 'string',
                            format: 'uuid',
                        },
                        pointTotal: {
                            type: 'integer'
                        },
                        rank: {
                            type: 'string'
                        }
                    }
                },
                Level: {
                    type: 'object',
                    properties: {
                        levelId: {
                            type: 'string',
                            format: 'uuid'
                        },
                        name: {
                            type: 'string'
                        },
                        scores: {
                            type: 'array',
                            items: {
                                $ref: '#/components/schemas/Score'
                            }
                        }
                    }
                },
                Error: {
                    type: 'object',
                    properties: {
                        message: {
                            type: 'string'
                        },
                        error: {
                            type: 'string'
                        }
                    }
                },
            },
            responses: {
                BadRequest: {
                    description: 'Dades invàlides',
                    content: {
                        'application/json': {
                            schema: {
                                $ref: '#/components/schemas/Error'
                            }
                        }
                    }
                },
                Unauthorized: {
                    description: 'No autoritzat',
                    content: {
                        'application/json': {
                            schema: {
                                $ref: '#/components/schemas/Error'
                            }
                        }
                    }
                },
                ServerError: {
                    description: 'Error intern del servidor',
                    content: {
                        'application/json': {
                            schema: {
                                $ref: '#/components/schemas/Error'
                            }
                        }
                    }
                }
            }
        },
        tags: [
            {
                name: 'Users',
                description: 'Gestió de usuaris i sessions'
            },
            {
                name: 'Scores',
                description: 'Gestió de puntuacions'
            },
        ]
    },
    apis: ['./src/routes/*.js']
};

module.exports = swaggerJsDoc(swaggerOptions);