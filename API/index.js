const dotenv = require('dotenv');
dotenv.config();

const cors        = require("cors");
const express     = require("express");
const fs          = require("fs");
const path        = require("path");
const yaml        = require("js-yaml");
const swaggerUi   = require("swagger-ui-express");
const { sequelize } = require("./src/config/database");
const swaggerSpec = require("./src/config/swagger");
const routes      = require("./src/routes");
const { logger, expressLogger } = require('./src/config/logger');
const { seedDatabase } = require('./data/seed/seed_database') 

const app  = express();
const PORT = parseInt(process.env.PORT, 10) || 3000;

// Middleware
  app.use(cors());
  app.use(express.json());

  // Servir fitxers estàtics (icones de perfil)
  const { ICONS_DIR } = require("./src/upload");
  app.use("/static/icons", express.static(ICONS_DIR));

// Swagger UI 
app.use("/docs", swaggerUi.serve, swaggerUi.setup(swaggerSpec));

// Rutes d'API
app.use("/", routes);

// Error 404
app.use((req, res) => res.status(404).json({ error: "Endpoint not found" }));


async function start() {
  // 1. Connectar MySQL i poblar de dades
  await sequelize.authenticate();
  console.log(">>> Base de dades connectada");

  const force = process.env.DB_SYNC_FORCE === "true";
  await seedDatabase(force);

  // 2. Actualitzar swagger.yaml
  const yamlPath = path.join(__dirname, "swagger.yaml");
  fs.writeFileSync(yamlPath, yaml.dump(swaggerSpec, { lineWidth: -1 }));
  console.log(`>>> swagger.yaml updated at ${yamlPath}`);

  // 3. Arrencar servidor
  app.listen(PORT, () => {
    console.log(`>>> Fruity Tunes API executant en http://localhost:${PORT}`);
    console.log(`>>> Swagger docs disponibles en http://localhost:${PORT}/docs`);
  });
}

start().catch((err) => {
  console.error("Failed to start:", err);
  process.exit(1);
});

module.exports = app;
