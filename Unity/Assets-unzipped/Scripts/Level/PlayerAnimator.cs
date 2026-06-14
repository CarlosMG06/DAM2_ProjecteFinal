using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    /*
        Animates the player character on beat (Metronome)
        Also animates the characters on either side of the player

        Animation depends on whether the players fails or succeeds each key press (Judge)
    */
    [SerializeField] private GameObject leftCharacter;
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private GameObject rightCharacter;
    private Animator leftAnimator;
    private Animator playerAnimator;
    private Animator rightAnimator;
    
    void Start()
    {
        leftAnimator = leftCharacter.GetComponent<Animator>();
        playerAnimator = playerCharacter.GetComponent<Animator>();
        rightAnimator = rightCharacter.GetComponent<Animator>();
    }

    void Update()
    {
        
    }
    
    public void OnLevelStart() {
        float bpm = LevelRunData.GetSong().GetBPM();
        float bps = bpm / 60;
        // Adjust animation speed (1 = 2 beats per second) to match the song BPM
        leftAnimator.SetFloat("speedMultiplier", bps / 2);
        playerAnimator.SetFloat("speedMultiplier", bps / 2);
        rightAnimator.SetFloat("speedMultiplier", bps / 2);
    }

    void OnPlayerKeySuccess()
    {
        // Animate player success
    }

    void OnPlayerKeyFailure()
    {
        // Animate player failure
    }

    void OnEnterBeat()
    {
        
    }
    void OnBeat()
    {
        
    }
    void OnExitBeat()
    {
        
    }
    void OnEnterHalfBeat()
    {
        
    }
    void OnHalfBeat()
    {
        
    }
    void OnExitHalfBeat()
    {
        
    }
}
