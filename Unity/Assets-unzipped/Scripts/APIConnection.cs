
using UnityEngine;
using System;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class APIConnection
{
   private const string API_URL = "http://localhost:3000/";

    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri(API_URL),
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly Lazy<APIConnection> _lazy = new(() => new APIConnection());
    public static APIConnection Instance => _lazy.Value;


    private APIConnection() { }


    [Serializable]
    private class ScorePayload
    {
        public string songId;
        public int highscore;
        public int maxCombo;
        public string rank;
    }

    public async Task<bool> SendScore()
    {
        try
        {
            var payload = new ScorePayload {
                songId    = LevelRunData.song.id,
                highscore = LevelRunData.score,
                maxCombo  = LevelRunData.maxCombo,
                rank      = LevelRunData.rank
            };

            var json    = JsonUtility.ToJson(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"scores/{GlobalGameData.playerId}", content);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"[APIConnection] SendScore failed: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("[APIConnection] SendScore timed out");
            return false;
        }
    }

    [Serializable]
    private class PlayerResponse { public string playerId; }

    public async Task<string> CreatePlayer(string playerName)
    {
        try
        {
            var json = $@"{{ ""playerName"": ""{playerName}"" }}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("players", content);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var data = JsonUtility.FromJson<PlayerResponse>(body);
            return data.playerId;
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"[APIConnection] CreatePlayer failed: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("[APIConnection] CreatePlayer timed out");
            return null;
        }
    }

    public async Task<List<LevelSaveData>> GetLevels(string songId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"scores/{songId}");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var raw = JsonUtility.FromJson<RawLeaderboard>(body);

            var levels = new List<LevelSaveData>();
            foreach (var entry in raw.leaderboardEntries)
                levels.Add(entry.ToLevelSaveData());

            return levels;
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"[APIConnection] GetLevels failed: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("[APIConnection] GetLevels timed out");
            return null;
        }
    }    

    public async Task<List<SongData>> GetSongs()
    {
        try
        {
            var response = await _httpClient.GetAsync("songs");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var wrapper = JsonUtility.FromJson<SongListWrapper>("{\"songs\":" + body + "}");

            var songs = new List<SongData>();
            foreach (var raw in wrapper.songs)
                songs.Add(raw.ToSongData());
            return songs;
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"[APIConnection] GetSongs failed: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("[APIConnection] GetSongs timed out");
            return null;
        }
    }
        
    // ── Deserialization types ─────────────────────────────────────────

    [Serializable]
    private class SongListWrapper
    {
        public List<RawSong> songs;
    }

    [Serializable]
    private class RawSong
    {
        public string songId;
        public string songTitle;
        public int bpm;
        public string audioFile;
        public float offset;
        public List<RawNote> chart;

        public SongData ToSongData()
        {
            string audioName = System.IO.Path.GetFileNameWithoutExtension(audioFile);
            
            var song = new SongData(
                songId, songTitle, bpm, 
                Resources.Load<AudioClip>($"Audio/Music/{audioName}"), 
                offset)
            {
                chart = BuildChart()
            };

            if (song.audio == null)
                Debug.LogError($"[APIConnection] AudioClip not found: Audio/Music/{audioName}");

            return song;
        }

        private SongChart BuildChart()
        {
            var inputs = new Dictionary<float, Key>();
            foreach (var note in chart)
                inputs[note.inputBeat] = MapInputKey(note.inputKey);

            return new SongChart(inputs);
        }

        private static Key MapInputKey(string inputKey) => inputKey.ToLower() switch
        {
            "left"  => Key.LeftArrow,
            "right" => Key.RightArrow,
            _       => throw new ArgumentException($"Unknown inputKey: '{inputKey}'")
        };
    }

    [Serializable]
    private class RawNote
    {
        public string id;
        public float inputBeat;
        public string inputKey;
    }

    [Serializable]
    private class RawLeaderboard
    {
        public string songId;
        public List<RawLeaderboardEntry> leaderboardEntries;
    }

    [Serializable]
    private class RawLeaderboardEntry
    {
        public int playerId;
        public string playerName;
        public int highscore;
        public int maxCombo;
        public string rank;

        public LevelSaveData ToLevelSaveData() => new LevelSaveData(playerName, highscore, rank);
    }
}
