using UnityEngine;
using UnityEngine.InputSystem;

public class Judge : MonoBehaviour
{
    /*
        Validates the player's actions and broadcasts Success or Failure.
        Supports both whole beats and half-beats (.5) from the chart.
    */

    private int? activeBeat;
    private int? activeHalfBeat;
    private int lastBeat;
    private int lastHalfBeat;
    public static Metronome metronome;
    private bool keySuccessThisBeat;
    private bool keySuccessThisHalfBeat;

    void Awake()
    {
        metronome = GetComponent<Metronome>();
    }

    void OnLeftArrowPressed()  { OnKeyPressed(Keyboard.current.leftArrowKey.keyCode); }
    void OnRightArrowPressed() { OnKeyPressed(Keyboard.current.rightArrowKey.keyCode); }

    void OnKeyPressed(Key keyCode)
    {
        var (nextInputBeat, nextInputKey) = LevelRunData.GetSong().GetChart().GetNextInput(lastBeat);

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
                BroadcastMessage("OnPlayerKeyFailure");
            }
        }
        else
        {
            // Whole-beat input
            int expectedBeat = Mathf.RoundToInt(nextInputBeat);
            if (expectedBeat == activeBeat && nextInputKey == keyCode)
            {
                EvaluateBeatTiming();
                keySuccessThisBeat = true;
            }
            else
            {
                BroadcastMessage("OnPlayerKeyFailure");
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
        int timePositionMs = LevelRunData.GetTimePositionMs();

        if (timePositionMs < startPosMs || timePositionMs > endPosMs)
        {
            Debug.LogWarning("Beat is active, but key press is outside of beat window. This should not happen.");
            return;
        }

        int distanceFromCenter = Mathf.Abs(timePositionMs - centerPosMs);
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

        BroadcastMessage("OnPlayerKeySuccess", (ratingIndex, scoreToAdd));
    }

    // --- Beat callbacks ---

    void OnEnterBeat()
    {
        activeBeat = metronome.GetActiveBeat();
        keySuccessThisBeat = false;
    }

    void OnExitBeat()
    {
        lastBeat = metronome.GetLastBeat();
        activeBeat = metronome.GetActiveBeat();

        // Only penalise a missed whole-beat if the chart actually expected one here
        var (nextInputBeat, _) = LevelRunData.GetSong().GetChart().GetNextInput(lastBeat - 1);
        bool wholeBeatExpected = (nextInputBeat == lastBeat && nextInputBeat % 1 == 0);
        if (wholeBeatExpected && !keySuccessThisBeat)
        {
            BroadcastMessage("OnPlayerKeyFailure");
        }
    }

    // --- Half-beat callbacks ---

    void OnEnterHalfBeat()
    {
        activeHalfBeat = metronome.GetActiveHalfBeat();
        keySuccessThisHalfBeat = false;
    }

    void OnExitHalfBeat()
    {
        lastHalfBeat = metronome.GetLastHalfBeat();
        activeHalfBeat = metronome.GetActiveHalfBeat();

        // Only penalise a missed half-beat if the chart actually expected one here
        var (nextInputBeat, _) = LevelRunData.GetSong().GetChart().GetNextInput(lastBeat - 0.5f);
        bool halfBeatExpected = (Mathf.RoundToInt(nextInputBeat * 2) == lastHalfBeat && nextInputBeat % 1 == 0.5f);
        if (halfBeatExpected && !keySuccessThisHalfBeat)
        {
            BroadcastMessage("OnPlayerKeyFailure");
        }
    }
}