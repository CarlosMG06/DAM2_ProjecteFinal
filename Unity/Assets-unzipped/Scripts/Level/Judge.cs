using UnityEngine;
using UnityEngine.InputSystem;

public class Judge : MonoBehaviour
{
    /*
        Validates the player's actions and broadcasts Success or Failure.
        Supports both whole beats and half-beats (.5) from the chart.
    */

    private int? activeBeat;
    private float? activeHalfBeat;
    private int? lastBeat = null;
    private float? lastHalfBeat = null;
    public static Metronome metronome;
    private bool keySuccessThisBeat;
    private bool keySuccessThisHalfBeat;

    public Synchronizer synchronizer;

    void Awake()
    {
        synchronizer = GetComponent<Synchronizer>();
        metronome = GetComponent<Metronome>();
    }

    public void OnLeftArrowPressed()  { OnKeyPressed(Keyboard.current.leftArrowKey.keyCode); }
    public void OnRightArrowPressed() { OnKeyPressed(Keyboard.current.rightArrowKey.keyCode); }

    void OnKeyPressed(Key keyCode)
    {
        var (nextInputBeat, nextInputKey) = LevelRunData.GetSong().GetChart().GetNextInput(lastBeat ?? 0-1);

        bool isHalfBeat = (nextInputBeat % 1 == 0.5f);

        if (isHalfBeat)
        {
            // Half-beat input: the chart beat e.g. 2.5 corresponds to half-beat index 5
            int expectedHalfBeat = Mathf.RoundToInt(nextInputBeat * 2);
            if (expectedHalfBeat == activeHalfBeat && nextInputKey == keyCode)
            {
                EvaluateHalfBeatTiming();
                keySuccessThisHalfBeat = true;
            }
            else
            {
                synchronizer.OnPlayerKeyFailure();
            }
        }
        else
        {
            // Whole-beat input
            int expectedBeat = Mathf.RoundToInt(nextInputBeat);
            print("expectedBeat: " + expectedBeat);
            print("activeBeat: " + activeBeat);
            if (expectedBeat == activeBeat)
            {
                EvaluateBeatTiming();
                keySuccessThisBeat = true;
            }
            else
            {
                synchronizer.OnPlayerKeyFailure();
            }
        }
    }

    void EvaluateBeatTiming()
    {
        int startPosMs  = metronome.GetActiveBeatStartPosMs();
        int centerPosMs = metronome.GetActiveBeatPosMs();
        int endPosMs    = metronome.GetActiveBeatEndPosMs();
        EvaluateTiming(startPosMs, centerPosMs, endPosMs);
    }

    void EvaluateHalfBeatTiming()
    {
        int startPosMs  = metronome.GetActiveHalfBeatStartPosMs();
        int centerPosMs = metronome.GetActiveHalfBeatPosMs();
        int endPosMs    = metronome.GetActiveHalfBeatEndPosMs();
        EvaluateTiming(startPosMs, centerPosMs, endPosMs);
    }

    void EvaluateTiming(int startPosMs, int centerPosMs, int endPosMs)
    {
        float timePositionMs = LevelRunData.GetTimePositionMs();

        if (timePositionMs < startPosMs || timePositionMs > endPosMs)
        {
            Debug.LogWarning("Beat is active, but key press is outside of beat window. This should not happen.");
            return;
        }

        float distanceFromCenter = Mathf.Abs(timePositionMs - centerPosMs);
        int marginMs = metronome.GetMarginMs();

        int ratingIndex;
        int scoreToAdd;
        if (distanceFromCenter <= marginMs * 0.2f)
        {
            ratingIndex = 3;
            scoreToAdd  = 100;
        }
        else if (distanceFromCenter <= marginMs * 0.5f)
        {
            ratingIndex = 2;
            scoreToAdd  = 70;
        }
        else
        {
            ratingIndex = 1;
            scoreToAdd  = 50;
        }

        synchronizer.OnPlayerKeySuccess(ratingIndex, scoreToAdd);
    }

    // --- Beat callbacks ---

    public void OnEnterBeat(int? activeBeat)
    {
        this.activeBeat = activeBeat;
        keySuccessThisBeat = false;
    }

    public void OnExitBeat(int? activeBeat)
    {

        lastBeat = activeBeat;

        // Only penalise a missed whole-beat if the chart actually expected one here
        var (nextInputBeat, _) = LevelRunData.GetSong().GetChart().GetNextInput(lastBeat ?? 0 -1);
        print("nextInputBeat: " + nextInputBeat);
        print("lastBeat: " + lastBeat);
        bool wholeBeatExpected = (nextInputBeat -1 == lastBeat && nextInputBeat % 1 == 0);
        if (wholeBeatExpected && !keySuccessThisBeat)
        {
            print("penalising whole-beat");
            synchronizer.OnPlayerKeyFailure();
        }
    }

    // --- Half-beat callbacks ---

    public void OnEnterHalfBeat(float? activeHalfBeat)
    {
        this.activeHalfBeat = activeHalfBeat;
        keySuccessThisHalfBeat = false;
    }

    public void OnExitHalfBeat(float? activeHalfBeat)
    {

        lastHalfBeat = activeHalfBeat;

        // Only penalise a missed half-beat if the chart actually expected one here
        var (nextInputBeat, _) = LevelRunData.GetSong().GetChart().GetNextInput((lastBeat ?? 0));
        print("lastHalfBeat: " + lastHalfBeat);
        bool halfBeatExpected = (nextInputBeat == lastHalfBeat - 1 && nextInputBeat % 1 == 0.5f);
        if (halfBeatExpected && !keySuccessThisHalfBeat)
        {
            print("penalising half-beat");
            synchronizer.OnPlayerKeyFailure();
        }
    }
}