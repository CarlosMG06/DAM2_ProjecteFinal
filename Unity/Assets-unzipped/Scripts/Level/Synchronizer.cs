using UnityEngine;

public class Synchronizer : MonoBehaviour
{
    /*
        Begins levels, starting up and syncing all game logic components
    
        Ends levels, stopping all game logic components and showing the results
        - When the song ends (AudioPlayer)
    */
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private Display display;

    void Start()
    {
        audioPlayer = GetComponent<AudioPlayer>();
        display = GetComponent<Display>();
        
        LevelRunData.Reset();
        BroadcastMessage("OnLevelStart");
    }

    async void OnSongEnd() {
        BroadcastMessage("OnLevelEnd");
        CalculateRank();
        await APIConnection.Instance.SendScore();
        display.ShowResults();
    }

    void CalculateRank() {
        int score = LevelRunData.GetScore();
        int maxScore = LevelRunData.GetSong().GetChart().GetTotalRequiredInputs() * 100;
        
        float percentage = (float)score / (float)maxScore;
        int missCount = LevelRunData.GetRatingCount(0);
        
        string rank;
        if (percentage > 0.9 && missCount == 0) { rank = "S"; } 
        else if (percentage > 0.9) { rank = "A"; }
        else if (percentage > 0.8) { rank = "B"; }
        else if (percentage > 0.6) { rank = "C"; }
        else { rank = "D"; }
        LevelRunData.SetRank(rank);
        display.UpdateRank();
    }
    public void OnPlayerKeySuccess(int ratingIndex, int scoreToAdd) {
        if (scoreToAdd > 0) {
            LevelRunData.AddScore(scoreToAdd);
            display.UpdateScore();
        }
        LevelRunData.IncreaseCombo();
        LevelRunData.IncreaseRatingCount(ratingIndex);
        StartCoroutine(display.ShowRating(ratingIndex, 150, 100));
        display.UpdateRatingCount(ratingIndex, LevelRunData.GetRatingCount(ratingIndex));
    }
    public void OnPlayerKeyFailure() {
        LevelRunData.ResetCombo();
        LevelRunData.IncreaseRatingCount(0);
        StartCoroutine(display.ShowRating(0, 150, 100));
        display.UpdateRatingCount(0, LevelRunData.GetRatingCount(0));
    }
}
