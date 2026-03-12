using UnityEngine;

public static class LevelRunData {
    /*
        SongData, Score, Time Position, isActive
    */

    private static SongData song;
    private static int score;
    private static int timePositionMs;
    private static bool isActive;

    static LevelRunData() {
        score = 0;
        timePositionMs = 0;
        isActive = false;
    }

    public static void StartLevelRun() {
        score = 0;
        timePositionMs = 0;
        isActive = true;
    }
    public static void EndLevelRun() {
        isActive = false;
    }
    public static bool GetIsActive() { return isActive; }

    public static void SetSong(SongData _song) { 
        song = _song; 
    } 
    public static SongData GetSong() { return song; }

    public static void AddScore(int scoreToAdd) {
        score += scoreToAdd;
    }
    public static int GetScore() { return score; }

    public static void SetTimePositionMs(int _timePositionMs) {
        timePositionMs = _timePositionMs;
    } 
    public static int GetTimePositionMs() { return timePositionMs; }
}

