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

    void OnSongEnd() {
        // Check in case of error
        float timePositionMs = LevelRunData.GetTimePositionMs();
        float currentSongLengthMs = audioPlayer.GetCurrentSongLengthMs();
        /*if (timePositionMs < currentSongLengthMs)
        {
            Debug.LogWarning("OnSongEnd called, but timePositionMs is " + timePositionMs + " and currentSongLengthMs is " + currentSongLengthMs);
            return;
        }*/
        
        BroadcastMessage("OnLevelEnd");
        APIConnection.Instance.SendScore();
        display.ShowResults();
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
