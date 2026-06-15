using UnityEngine;

public class LevelRunData {
    /*
        SongData, Score, Time Position
    */

    public static SongData song;
    public static int score;
    public static int combo;
    public static int maxCombo;
    public static int[] ratingCounts = new int[4]; // Miss, Ok, Great, Perfect
    public static float timePositionMs;
    public static string rank = "";
    
    static LevelRunData() {}

    public static void Reset() {
        score = 0;
        timePositionMs = 0f;
        combo = 0;
        maxCombo = 0;
        rank = "";
        for (int i = 0; i < ratingCounts.Length; i++) {
            ratingCounts[i] = 0;
        }
    }

    public static void SetSong(SongData _song) { 
        song = _song; 
    } 
    public static SongData GetSong() { return song; }

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

    public static void SetTimePositionMs(float _timePositionMs) {
        timePositionMs = _timePositionMs;
    } 
    public static float GetTimePositionMs() { return timePositionMs; }

    public static void SetRank(string _rank) {
        rank = _rank;
    }
    public static string GetRank() { return rank; }
}

