const API_BASE_URL = 'http://localhost:3000';

let songs = [];
let players = [];
let currentSongId = null;
let currentPlayerId = null;

// ==================== API ====================

async function fetchAPI(endpoint) {
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`);
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.error || `HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Error fetching ${endpoint}:`, error);
        throw error;
    }
}

// Cargar todas las canciones
async function loadSongs() {
    try {
        const data = await fetchAPI('/songs');
        songs = Array.isArray(data) ? data : [];
        renderSongs(songs);
    } catch (error) {
        document.getElementById('songsList').innerHTML = 
            '<div class="error">Error loading songs</div>';
        console.error('Error loading songs:', error);
    }
}

// Cargar todos los jugadores
async function loadPlayers() {
    try {
        const data = await fetchAPI('/players');
        players = data.players || (Array.isArray(data) ? data : []);
        renderPlayers(players);
    } catch (error) {
        document.getElementById('playersList').innerHTML = 
            '<div class="error">Error loading players</div>';
        console.error('Error loading players:', error);
    }
}

async function loadScoresBySong(songId, songTitle) {
    try {
        const data = await fetchAPI(`/scores/${songId}`);
        showSongDetails(songTitle, data);
    } catch (error) {
        showError('Couldn\'t load scores for this song');
    }
}

async function loadPlayerDetails(playerId, playerName) {
    try {
        const data = await fetchAPI(`/players/${playerId}`);
        showPlayerDetails(playerName, data);
    } catch (error) {
        showError('Couldn\'t load details for this player');
    }
}

// ==================== RENDERITZACIÓ ====================

function renderSongs(songsList) {
    const container = document.getElementById('songsList');
    if (!songsList || songsList.length === 0) {
        container.innerHTML = '<div class="loading">No available songs</div>';
        return;
    }

    container.innerHTML = songsList.map(song => `
        <div class="item ${currentSongId === song.songId ? 'active' : ''}" 
                onclick="selectSong('${song.songId}', '${escapeHtml(song.songTitle)}')">
            <h3>${escapeHtml(song.songTitle)}</h3>
            <div class="info">
                <span class="badge">BPM: ${song.bpm}</span>
                ${song.audioFile ? `<span class="badge">Arxiu: ${escapeHtml(song.audioFile)}</span>` : ''}
            </div>
        </div>
    `).join('');
}

function renderPlayers(playersList) {
    const container = document.getElementById('playersList');
    if (!playersList || playersList.length === 0) {
        container.innerHTML = '<div class="loading">No available players</div>';
        return;
    }

    container.innerHTML = playersList.map(player => `
        <div class="item ${currentPlayerId === player.playerId ? 'active' : ''}" 
                onclick="selectPlayer('${player.playerId}', '${escapeHtml(player.playerName)}')">
            <div class="player-row">
                <img class="player-icon"
                     src="${player.playerIcon || ''}"
                     alt="${escapeHtml(player.playerName)}"
                     onerror="this.style.display='none';this.nextElementSibling.style.display='flex'">
                <div class="player-icon-fallback" style="display:none">
                    ${escapeHtml(player.playerName.charAt(0).toUpperCase())}
                </div>
                <div class="player-info">
                    <h3>${escapeHtml(player.playerName)}</h3>
                </div>
            </div>
        </div>
    `).join('');
}

function showSongDetails(songTitle, data) {
    const detailsPanel = document.getElementById('detailsPanel');
    const detailsTitle = document.getElementById('detailsTitle');
    const detailsContent = document.getElementById('detailsContent');
    
    detailsTitle.innerHTML = `${escapeHtml(songTitle)} - Leaderboard`;
    
    // API retorna { songId, leaderboardEntries: [...] }
    const entries = data.leaderboardEntries || [];
    if (entries.length === 0) {
        detailsContent.innerHTML = `
            <div class="loading">No scores for this song yet</div>
        `;
    } else {
        detailsContent.innerHTML = `
            <div class="stats">Top ${entries.length} players</div>
            <div class="scores-table">
                <table>
                    <thead>
                        <tr>
                            <th>#</th>
                            <th>Player</th>
                            <th>Highscore</th>
                            <th>Max Combo</th>
                            <th>Rank</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${entries.map((entry, index) => `
                            <tr>
                                <td>${index + 1}</td>
                                <td><strong>${escapeHtml(entry.playerName)}</strong></td>
                                <td>${entry.highscore.toLocaleString()}</td>
                                <td>${entry.maxCombo}</td>
                                <td><span class="rank-badge rank-${entry.rank}">${entry.rank}</span></td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }
    
    detailsPanel.style.display = 'block';
    detailsPanel.scrollIntoView({ behavior: 'smooth' });
}

function showPlayerDetails(playerName, data) {
    const detailsPanel = document.getElementById('detailsPanel');
    const detailsTitle = document.getElementById('detailsTitle');
    const detailsContent = document.getElementById('detailsContent');
    
    const iconUrl = data.playerIcon || '';
    detailsTitle.innerHTML = `
        <span class="details-player-header">
            <img class="player-icon-lg"
                 src="${iconUrl}"
                 alt="${escapeHtml(playerName)}"
                 onerror="this.style.display='none';this.nextElementSibling.style.display='flex'">
            <span class="player-icon-lg-fallback" style="display:none">
                ${escapeHtml(playerName.charAt(0).toUpperCase())}
            </span>
            ${escapeHtml(playerName)} - Stats
        </span>`;
    
    // API retorna { playerId, playerName, levelScores: [{ songId, scores: [...] }] }
    const levelScores = data.levelScores || [];
    if (levelScores.length === 0) {
        detailsContent.innerHTML = `
            <div class="loading">This player doesn't have scores yet</div>
        `;
    } else {
        // Calcular estadístiques totals
        let totalHighscore = 0;
        let bestCombo = 0;
        let scoreCount = 0;

        levelScores.forEach(entry => {
            (entry.scores || []).forEach(score => {
                totalHighscore += score.highscore;
                if (score.maxCombo > bestCombo) bestCombo = score.maxCombo;
                scoreCount++;
            });
        });

        const avgHighscore = scoreCount > 0 ? totalHighscore / scoreCount : 0;

        // Trobar el títol de la cançó de la llista ja carregada, fallback a UUID abreujada
        const songTitle = (songId) => {
            const found = songs.find(s => s.songId === songId);
            return found ? found.songTitle : songId.substring(0, 8) + '…';
        };

        detailsContent.innerHTML = `
            <div class="stats">
                Total Highscore: ${totalHighscore.toLocaleString()} | 
                Average: ${Math.floor(avgHighscore).toLocaleString()} |
                Best Combo: ${bestCombo}
            </div>
            <div class="scores-table">
                <table>
                    <thead>
                        <tr>
                            <th>Song</th>
                            <th>Score</th>
                            <th>Max Combo</th>
                            <th>Rank</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${levelScores.flatMap(entry =>
                            (entry.scores || []).map(score => `
                                <tr>
                                    <td><strong>${escapeHtml(songTitle(entry.songId))}</strong></td>
                                    <td>${score.highscore.toLocaleString()}</td>
                                    <td>${score.maxCombo}</td>
                                    <td><span class="rank-badge rank-${score.rank}">${score.rank}</span></td>
                                </tr>
                            `)
                        ).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }
    
    detailsPanel.style.display = 'block';
    detailsPanel.scrollIntoView({ behavior: 'smooth' });
}

function showError(message) {
    const detailsPanel = document.getElementById('detailsPanel');
    const detailsTitle = document.getElementById('detailsTitle');
    const detailsContent = document.getElementById('detailsContent');
    detailsTitle.innerHTML = '❌ Error';
    detailsContent.innerHTML = `<div class="error">${message}</div>`;
    detailsPanel.style.display = 'block';
}

function closeDetails() {
    document.getElementById('detailsPanel').style.display = 'none';
    currentSongId = null;
    currentPlayerId = null;
    renderSongs(songs);
    renderPlayers(players);
}

// ==================== INTERACCIÓ ====================

function selectSong(songId, songTitle) {
    currentSongId = songId;
    currentPlayerId = null;
    renderSongs(songs);
    renderPlayers(players);
    loadScoresBySong(songId, songTitle);
}

function selectPlayer(playerId, playerName) {
    currentPlayerId = playerId;
    currentSongId = null;
    renderSongs(songs);
    renderPlayers(players);
    loadPlayerDetails(playerId, playerName);
}

function filterSongs() {
    const searchTerm = document.getElementById('searchSong').value.toLowerCase();
    const filtered = songs.filter(song => 
        song.songTitle.toLowerCase().includes(searchTerm)
    );
    renderSongs(filtered);
}

function filterPlayers() {
    const searchTerm = document.getElementById('searchPlayer').value.toLowerCase();
    const filtered = players.filter(player => 
        player.playerName.toLowerCase().includes(searchTerm)
    );
    renderPlayers(filtered);
}

// Escapar caràcters en HTML 
function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

// ==================== INICIALITZACIÓ ====================

async function init() {
    await loadSongs();
    await loadPlayers();
}

init();