/**
 * Configuració principal del servidor Express
 * Aquest fitxer inicialitza tots els components necessaris per l'API
 */

// Carregar variables d'entorn
const dotenv = require('dotenv');
dotenv.config();

// Importacions principals
const cors = require('cors');
const express = require('express');
const swaggerUi = require('swagger-ui-express');
const fs = require('fs');
const YAML = require('yamljs');

const swaggerSpecs = require('./src/swagger');
const { sequelize } = require('./src/config/database');
const routes = require('./src/routes');

// Crear instància d'Express
const app = express();

/**
 * Configuració dels middlewares principals
 * - CORS per permetre peticions des d'altres dominis
 * - Parser de JSON per processar el cos de les peticions
 */
app.use(cors());
app.use(express.json());

// Configuració de Swagger per la documentació de l'API
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerSpecs));

app.get('/api-docs-json', (req, res) => {
    res.setHeader('Content-Type', 'application/json');
    res.send(swaggerSpecs);
});

app.get('/api-docs-yaml', (req, res) => {
    res.setHeader('Content-Type', 'text/yaml');
    // Convertim l'objecte swaggerSpecs a string YAML
    const yamlSpecs = YAML.stringify(swaggerSpecs, 10); 
    res.send(yamlSpecs);
});

const yamlContent = YAML.stringify(swaggerSpecs, 10);
fs.writeFileSync('./swagger.yaml', yamlContent);

console.log('Fitxer swagger.yaml actualitzat correctament');

// Registre de les rutes principals
app.use('/api', routes);

// Port per defecte 3000 si no està definit a les variables d'entorn
const PORT = process.env.PORT || 3000;

/**
 * Funció d'inicialització del servidor
 * - Connecta amb la base de dades
 * - Sincronitza els models
 * - Inicia el servidor HTTP
 */
async function startServer() {
    try {
        // Verificar connexió amb la base de dades
        await checkDatabaseConnection();
        
        // Iniciar el servidor HTTP
        app.listen(PORT, () => {
            console.log(`Servidor escoltant en http://0.0.0.0:${port}`)
        });

    } catch (error) {
        console.error("Error en iniciar el servidor:", error)
        process.exit(1);
    }
}
async function checkDatabaseConnection() {
    try {
        await sequelize.authenticate();
        console.log("Base de dades connectada")
    } catch (error) {
        console.error("Error al connectar a la BBDD:", error)
    }
}

// Iniciar el servidor
startServer();


// Gestió del senyal SIGTERM per tancament graciós
process.on('SIGINT', shutdown)
process.on('SIGTERM', shutdown);
function shutdown() {
    console.log('Received kill signal, shutting down gracefully');
    httpServer.close(() => {
        console.log('Server closed');
        process.exit(0);
    });
}

module.exports = app;