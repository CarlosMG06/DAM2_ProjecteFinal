using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public static class GlobalGameData {
    /*
        Song Data:
            Title, BPM, Audio, Offset, Chart

        Options Save Data:
            Music Volume, SFX Volume
        
        Level Save Data:
            Highscore(s), Rank(s)
    */

    private static List<SongData> songs;
    private static OptionsData options;
    private static List<LevelSaveData> levels;

    public static List<SongData> GetSongs() { return songs; }
    public static OptionsData GetOptions() { return options; }
    public static List<LevelSaveData> GetLevels() { return levels; }
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

    OptionsData() {
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
    private int highScore;
    private int rank;

    public void SetHighscore(int highScore) {
        this.highScore = highScore;
    }
    public int GetHighscore() { return highScore; }

    public void SetRank(int rank) {
        this.rank = rank;
    }
    public int GetRank() { return rank; }
}
