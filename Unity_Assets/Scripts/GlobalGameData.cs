using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class GlobalGameData {
    /*
        Song Data:
            Title, BPM, Audio, Offset, Chart

        Options Save Data:
            Music Volume, SFX Volume, Language?, Keymapping?
        
        Level Save Data:
            Highscore(s), Rank(s)
        
        Player Data:
            ID, Name
    */

    private static List<SongData> songs;
    private static OptionsData options;
    private static List<LevelSaveData> levels;
    private static List<PlayerData> players;

    static GlobalGameData() {
        songs = new List<SongData>();
        songs.Add(new SongData(
            "Must, Be Nice",
            105,
            Resources.Load<AudioClip>("Audio/Music/Must, Be Nice")
        ));
        songs.Add(new SongData(
            "Climbing Up The Wrong Vine",
            110,
            Resources.Load<AudioClip>("Audio/Music/Climbing Up The Wrong Vine (WIP)")
        ));
        options = new OptionsData();
        levels = new List<LevelSaveData>();
        levels.Add(new LevelSaveData());
        levels.Add(new LevelSaveData());
        players = new List<PlayerData>();
        players.Add(new PlayerData());
    }

    public static List<SongData> GetSongs() { return songs; }
    public static SongData GetSongFromTitle(string title) {
        return songs.Where(song => song.GetTitle() == title).ToList()[0];
    }
    public static OptionsData GetOptions() { return options; }
    public static List<LevelSaveData> GetLevels() { return levels; }
    public static List<PlayerData> GetPlayers() { return players; }
}

public class SongData {
    private string title;
    private int bpm;
    private AudioClip audio;
    private int offsetMs;
    private SongChart chart;

    public SongData(string title, int bpm, AudioClip audio) {
        this.title = title;
        this.bpm = bpm;
        this.audio = audio;
    }

    public string GetTitle() { return title; }
    public int GetBPM() { return bpm; }
    public AudioClip GetAudio() { return audio; }

    public void SetOffsetMs(int offsetMs) {
        this.offsetMs = offsetMs;
    }
    public int GetOffsetMs() { return offsetMs; }

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
    private int highScore;
    private string rank;

    public LevelSaveData() {
        this.highScore = 0;
        this.rank = "";
    }

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

    public PlayerData() {
        this.playerID = System.Guid.NewGuid().ToString();
        this.playerName = "Player";
    }

    public string GetPlayerID() { return playerID; }
    public string GetPlayerName() { return playerName; }
    public void SetPlayerName(string name) { playerName = name; }
}
