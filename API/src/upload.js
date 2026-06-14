// Script per pujar arxius amb la llibreria multer (segons GeeksForGeeks)
const multer = require("multer");
const path   = require("path");
const fs     = require("fs");

const ICONS_DIR = path.join(__dirname, "..", "data", "uploads", "icons");
if (!fs.existsSync(ICONS_DIR)) {
  fs.mkdirSync(ICONS_DIR, { recursive: true });
}

const fileFilter = (_req, file, cb) => {
  const filetypes = /jpeg|jpg|png/;
  const mimetype = filetypes.test(file.mimetype);
  const extname = filetypes.test(path.extname(file.originalname).toLowerCase());

  if (mimetype && extname) {
      return cb(null, true);
  }

  cb("Error: File upload only supports the following filetypes - " + filetypes);
};

// Mantenir arxiu en memòria per donar-li un nom després de generar la UUID
const upload = multer({
  storage:    multer.memoryStorage(),
  fileFilter,
  limits: { fileSize: 10 * 1024 * 1024 }, // 10 MB max
});

function saveIcon(playerId, buffer, mimetype) {
  const ext = mimetype === "image/jpeg" ? ".jpg"
            : mimetype === "image/webp" ? ".webp"
            : mimetype === "image/gif"  ? ".gif"
            : ".png";
  const filename = `${playerId}${ext}`;
  fs.writeFileSync(path.join(ICONS_DIR, filename), buffer);
  return filename;
}

module.exports = { upload, saveIcon, ICONS_DIR };
