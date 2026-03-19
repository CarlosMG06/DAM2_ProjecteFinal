using UnityEngine;
using UnityEngine.InputSystem;

public class Judge : MonoBehaviour
{
    /*
        Validates the player's actions and broadcasts Success or Failure
    */

    private int? activeBeat;
    private int lastBeat;
    public static Metronome Metronome;

    void Awake()
    {
        Metronome = GetComponent<Metronome>();
    }

    
    void OnLeftArrowPressed() { OnKeyPressed(Keyboard.current.leftArrowKey.keyCode); }
    void OnRightArrowPressed() { OnKeyPressed(Keyboard.current.rightArrowKey.keyCode); }

    void OnKeyPressed(Key keyCode)
    {
        if (LevelRunData.GetIsActive()) {
            var (nextInputBeat, nextInputKey) = LevelRunData.GetSong().GetChart().GetNextInput(lastBeat);
            if (nextInputBeat == activeBeat && nextInputKey == keyCode) {
                BroadcastMessage("OnPlayerKeySuccess", SendMessageOptions.DontRequireReceiver);
            } else {
                BroadcastMessage("OnPlayerKeyFailure", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void EnterBeat()
    {
        activeBeat = Metronome.GetActiveBeat();
    }

    void ExitBeat()
    {
        lastBeat = Metronome.GetLastBeat();
        activeBeat = Metronome.GetActiveBeat();
        if (LevelRunData.GetIsActive()) {
            BroadcastMessage("OnPlayerKeyFailure", SendMessageOptions.DontRequireReceiver);
        }
    }
}
