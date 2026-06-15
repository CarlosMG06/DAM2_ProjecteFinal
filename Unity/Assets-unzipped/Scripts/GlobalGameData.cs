using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GlobalGameData {
    /*
        Song Data:
            Title, BPM, Audio, Offset, Chart

        Options Save Data:
            Music Volume, SFX Volume
        
        Level Save Data:
            Highscore(s), Rank(s)
        
        Player Data:
            ID, Name
    */

    private static List<SongData> songs;
    private static OptionsData options;
    private static Dictionary<string, List<LevelSaveData>> levels;
    private static string playerName;
    public static string playerId;
    private static List<PlayerData> players;

    static GlobalGameData() {
        songs = new List<SongData>();
        options = new OptionsData();
        levels = new Dictionary<string, List<LevelSaveData>>();
    }

    public static async Task Initialize()
    {
        await CreatePlayer();
        await GetSongsFromAPI();
        await GetLevelsFromAPI();
    }

    private static async Task GetLevelsFromAPI() {
        foreach (SongData song in songs) {
            List<LevelSaveData> apiLevels = await APIConnection.Instance.GetLevels(song.id);
            if (apiLevels != null) {
                levels[song.id] = new List<LevelSaveData>();
                levels[song.id].AddRange(apiLevels);
            }
        }
    }

    private static async Task CreatePlayer() {
        playerName = "Player" + UnityEngine.Random.Range(1, 1000000);
        playerId = await APIConnection.Instance.CreatePlayer(playerName);
        if (playerId == null) {
            Debug.LogError("Failed to create player");
        }
    }

    private static async Task GetSongsFromAPI() {
        List<SongData> apiSongs = await APIConnection.Instance.GetSongs();
        if (apiSongs != null) {
            songs.AddRange(apiSongs);
        }
    }

    public static List<SongData> GetSongs() { return songs; }
    public static SongData GetSongFromTitle(string title) {
        return songs.Where(song => song.GetTitle() == title).ToList()[0];
    }
    public static OptionsData GetOptions() { return options; }
    public static Dictionary<string, List<LevelSaveData>> GetLevels() { return levels; }
    public static List<PlayerData> GetPlayers() { return players; }

    public static string GetPlayerName() { return playerName; }
}

public class SongData {
    public string id;
    public string title;
    public int bpm;
    public AudioClip audio;
    public float offsetMs;
    public SongChart chart;

    public SongData(string id, string title, int bpm, AudioClip audio, float offsetMs) {
        this.id = id;
        this.title = title;
        this.bpm = bpm;
        this.audio = audio;
    }

    public string GetId() { return id; }
    public string GetTitle() { return title; }
    public int GetBPM() { return bpm; }
    public AudioClip GetAudio() { return audio; }

    public void SetOffsetMs(float offsetMs) {
        this.offsetMs = offsetMs;
    }
    public float GetOffsetMs() { return offsetMs; }

    public void SetChart(SongChart chart) {
        this.chart = chart;
    }
    public SongChart GetChart() { return chart; }
}

public class OptionsData {
    private float musicVolume;
    private float sfxVolume;

    public OptionsData() {
        this.musicVolume = 1f;
        this.sfxVolume = 1f;
    }
    
    public void SetMusicVolume(float musicVolume) {
        this.musicVolume = musicVolume;
    }
    public float GetMusicVolume() { return musicVolume; }
    
    public void SetSFXVolume(float sfxVolume) {
        this.sfxVolume = sfxVolume;
    }
    public float GetSFXVolume() { return sfxVolume; }
}

public class LevelSaveData {

    private string playerName;
    private int highScore;
    private string rank;

    public LevelSaveData() {
        this.highScore = 0;
        this.rank = "";
    }

    public LevelSaveData(string playerName, int highScore, string rank)
    {
        this.playerName = playerName;
        this.highScore  = highScore;
        this.rank       = rank;
    }

    public void SetPlayerName(string playerName) {
        this.playerName = playerName;
    }
    public string GetPlayerName() { return playerName; }

    public void SetHighscore(int highScore) {
        this.highScore = highScore;
    }
    public int GetHighscore() { return highScore; }

    public void SetRank(string rank) {
        this.rank = rank;
    }
    public string GetRank() { return rank; }
}

public class PlayerData {
    private string playerID;
    private string playerName;

    public PlayerData(string _playerID, string _playerName) {
        this.playerID = _playerID;
        this.playerName = _playerName;
    }

    public string GetPlayerID() { return playerID; }
    public string GetPlayerName() { return playerName; }
    public void SetPlayerName(string name) { playerName = name; }
}

public class SongChart {
    private Dictionary<float, Key> requiredInputs;
    private float lastInputBeat;

    public SongChart(Dictionary<float, Key> requiredInputs) {
        this.requiredInputs = requiredInputs;
        float maxBeat = 0;
        foreach (var beat in requiredInputs.Keys) {
            maxBeat = Math.Max(maxBeat, beat);
        }
        this.lastInputBeat = maxBeat;
    }

    public (float, Key?) GetNextInput(float lastBeat) {
        float nextInputBeat = lastBeat + 0.5f;
        while (!requiredInputs.ContainsKey(nextInputBeat)) {
            nextInputBeat += 0.5f;
            if (nextInputBeat > lastInputBeat) {
                return (-1, null);
            }
        }
        Key nextInputKey = requiredInputs[nextInputBeat];
        return (nextInputBeat, nextInputKey);
    }

    public int GetTotalRequiredInputs() {
        return requiredInputs.Count;
    }
}