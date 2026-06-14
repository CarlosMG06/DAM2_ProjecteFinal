using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GlobalGameData {
    /*
        Song Data:
            Title, BPM, Audio, Offset, Chart

        Options Save Data:
            Music Volume, SFX Volume, Language?, Keymapping?
        
        Level Save Data:
            Highscore(s), Rank(s)
        
        Player Data:
            ID, Name, IconPath
    */

    private static List<SongData> songs;
    private static OptionsData options;
    private static Dictionary<int, List<LevelSaveData>> levels;
    private static string playerName;
    public static int playerId;
    private static List<PlayerData> players;

    static GlobalGameData() {
        CreatePlayer();
        songs = new List<SongData>();
        songs.Add(new SongData(
            1,
            "Must, Be Nice",
            105,
            Resources.Load<AudioClip>("Audio/Music/Must, Be Nice"),
            0f
        ));
        songs.Add(new SongData(
            2,
            "Climbing Up The Wrong Vine",
            110,
            Resources.Load<AudioClip>("Audio/Music/Climbing Up The Wrong Vine (WIP)"),
            0f
        ));
        GetSongsFromAPI();
        options = new OptionsData();
        levels = new Dictionary<int, List<LevelSaveData>>();
        levels[0] = new List<LevelSaveData>();
        levels[0].Add(new LevelSaveData(
            "Player1",
            10000,
            "SS"
        ));
        levels[0].Add(new LevelSaveData(
            "Player3",
            3000,
            "B"
        ));
        levels[1] = new List<LevelSaveData>();
        levels[1].Add(new LevelSaveData(
            "Player4",
            4000,
            "C"
        ));
        levels[1].Add(new LevelSaveData(
            "Player5",
            5000,
            "D"
        ));
        GetLevelsFromAPI();
    }

    private static async Task GetLevelsFromAPI() {
        foreach (SongData song in songs) {
            List<LevelSaveData> apiLevels = await APIConnection.Instance.GetLevels(song.id);
            if (apiLevels != null) {
                levels[song.id].AddRange(apiLevels);
            }
        }
    }

    private static async Task CreatePlayer() {
        playerName = "Player" + Random.Range(1, 1000000);
        playerId = await APIConnection.Instance.CreatePlayer(playerName);
        if (playerId == -1) {
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
    public static Dictionary<int, List<LevelSaveData>> GetLevels() { return levels; }
    public static List<PlayerData> GetPlayers() { return players; }
}

public class SongData {
    public int id;
    public string title;
    public int bpm;
    public AudioClip audio;
    public float offsetMs;
    public SongChart chart;

    public SongData(int id, string title, int bpm, AudioClip audio, float offsetMs) {
        this.id = id;
        this.title = title;
        this.bpm = bpm;
        this.audio = audio;
    }

    public int GetId() { return id; }
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

    //public enum Language {
    //    ENG, ESP, CAT
    //};
    //private Language language;

    public OptionsData() {
        this.musicVolume = 1f;
        this.sfxVolume = 1f;
        // this.language = Language.ENG;
    }
    
    public void SetMusicVolume(float musicVolume) {
        this.musicVolume = musicVolume;
    }
    public float GetMusicVolume() { return musicVolume; }
    
    public void SetSFXVolume(float sfxVolume) {
        this.sfxVolume = sfxVolume;
    }
    public float GetSFXVolume() { return sfxVolume; }

    //public void SetLanguage(Language language) {
    //    this.language = language;
    //}
    //public Language GetLanguage() { return language; }

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
    private string iconPath;

    public PlayerData(string _playerID, string _playerName, string _iconPath) {
        this.playerID = _playerID;
        this.playerName = _playerName;
        this.iconPath = _iconPath;
    }

    public string GetPlayerID() { return playerID; }
    public string GetPlayerName() { return playerName; }
    public void SetPlayerName(string name) { playerName = name; }
}
