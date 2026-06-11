using UnityEngine;

public static class LevelRunData {
    /*
        SongData, Score, Time Position
    */

    private static SongData song;
    private static int songIndex;
    private static int score;
    private static int combo;
    private static int maxCombo;
    private static int[] ratingCounts = new int[4]; // Miss, Ok, Great, Perfect
    private static int timePositionMs;
    
    static LevelRunData() {}

    public static void Reset() {
        score = 0;
        timePositionMs = 0;
        combo = 0;
        maxCombo = 0;
        for (int i = 0; i < ratingCounts.Length; i++) {
            ratingCounts[i] = 0;
        }
    }

    public static void SetSong(SongData _song) { 
        song = _song; 
    } 
    public static SongData GetSong() { return song; }

    public static void SetSongIndex(int _songIndex) {
        songIndex = _songIndex;
    }

    public static int GetSongIndex() { return songIndex; }

    public static void AddScore(int scoreToAdd) {
        score += scoreToAdd;
    }
    public static int GetScore() { return score; }

    public static void IncreaseCombo() {
        combo++;
        if (combo > maxCombo) {
            maxCombo = combo;
        }
    }
    public static void ResetCombo() { combo = 0; }
    public static int GetCombo() { return combo; }
    public static int GetMaxCombo() { return maxCombo; }

    public static void IncreaseRatingCount(int ratingIndex) {
        if (ratingIndex >= 0 && ratingIndex < ratingCounts.Length) {
            ratingCounts[ratingIndex]++;
        }
    }
    public static int GetRatingCount(int ratingIndex) {
        if (ratingIndex >= 0 && ratingIndex < ratingCounts.Length) {
            return ratingCounts[ratingIndex];
        }
        return 0;
    }

    public static void SetTimePositionMs(int _timePositionMs) {
        timePositionMs = _timePositionMs;
    } 
    public static int GetTimePositionMs() { return timePositionMs; }
}

