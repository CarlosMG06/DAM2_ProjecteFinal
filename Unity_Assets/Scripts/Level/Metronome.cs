using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Metronome : MonoBehaviour
{
    /*
        Keeps track of the beat and half-beat of the song
    */

    private int bpm;
    private int offsetMs;
    private const int marginMs = 80;
    private int beatDurationMs;
    private int halfBeatDurationMs;

    // Beat state
    private int lastBeat;
    private int nextBeatPosMs;
    private int? activeBeat;
    private int nextBeatStartPosMs;
    private int? activeBeatStartPosMs;
    private int? activeBeatPosMs;
    private int? activeBeatEndPosMs;

    // Half-beat state
    private int lastHalfBeat;
    private int nextHalfBeatPosMs;
    private int? activeHalfBeat;
    private int nextHalfBeatStartPosMs;
    private int? activeHalfBeatStartPosMs;
    private int? activeHalfBeatPosMs;
    private int? activeHalfBeatEndPosMs;

    void OnLevelStart()
    {
        bpm = LevelRunData.GetSong().GetBPM();
        offsetMs = LevelRunData.GetSong().GetOffsetMs();
        beatDurationMs = 60000 / bpm;
        halfBeatDurationMs = beatDurationMs / 2;

        // Beat initialisation
        lastBeat = 0;
        nextBeatPosMs = offsetMs + beatDurationMs;
        nextBeatStartPosMs = nextBeatPosMs - marginMs;
        activeBeatStartPosMs = null;
        activeBeatPosMs = null;
        activeBeatEndPosMs = null;
        activeBeat = null;

        // Half-beat initialisation — first half-beat sits halfway between beat 0 and beat 1
        lastHalfBeat = 0;
        nextHalfBeatPosMs = offsetMs + halfBeatDurationMs;
        nextHalfBeatStartPosMs = nextHalfBeatPosMs - marginMs;
        activeHalfBeatStartPosMs = null;
        activeHalfBeatPosMs = null;
        activeHalfBeatEndPosMs = null;
        activeHalfBeat = null;

        StartCoroutine(Tick());
    }

    void OnLevelEnd()
    {
        StopCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (true)
        {
            int timePositionMs = LevelRunData.GetTimePositionMs();

            // --- Beat logic ---

            if (timePositionMs >= nextBeatStartPosMs)
            {
                activeBeat = lastBeat + 1;

                activeBeatStartPosMs = nextBeatStartPosMs;
                activeBeatPosMs     = nextBeatStartPosMs + marginMs;
                activeBeatEndPosMs  = activeBeatStartPosMs + marginMs * 2;

                BroadcastMessage("OnEnterBeat", activeBeat);
                nextBeatStartPosMs += beatDurationMs;
            }
            if (timePositionMs >= nextBeatPosMs)
            {
                BroadcastMessage("OnBeat", activeBeat);
                // Debug
                print("Beat " + activeBeat + " at " + timePositionMs + "ms");
                nextBeatPosMs += beatDurationMs;
            }
            if (timePositionMs >= activeBeatEndPosMs)
            {
                lastBeat += 1;
                activeBeat = null;
                BroadcastMessage("OnExitBeat", activeBeat);
                activeBeatEndPosMs += beatDurationMs;
            }

            // --- Half-beat logic ---

            if (timePositionMs >= nextHalfBeatStartPosMs)
            {
                activeHalfBeat = lastHalfBeat + 1;

                activeHalfBeatStartPosMs = nextHalfBeatStartPosMs;
                activeHalfBeatPosMs      = nextHalfBeatStartPosMs + marginMs;
                activeHalfBeatEndPosMs   = activeHalfBeatStartPosMs + marginMs * 2;

                BroadcastMessage("OnEnterHalfBeat", activeHalfBeat);
                nextHalfBeatStartPosMs += halfBeatDurationMs;
            }
            if (timePositionMs >= nextHalfBeatPosMs)
            {
                BroadcastMessage("OnHalfBeat", activeHalfBeat);
                nextHalfBeatPosMs += halfBeatDurationMs;
            }
            if (timePositionMs >= activeHalfBeatEndPosMs)
            {
                lastHalfBeat += 1;
                activeHalfBeat = null;
                BroadcastMessage("OnExitHalfBeat", activeHalfBeat);
                activeHalfBeatEndPosMs += halfBeatDurationMs;
            }

            yield return null;
        }
    }

    // Beat getters
    public int  GetLastBeat()               { return lastBeat; }
    public int? GetActiveBeat()             { return activeBeat; }
    public int  GetMarginMs()               { return marginMs; }
    public int  GetActiveBeatStartPosMs()   { return activeBeatStartPosMs ?? 0; }
    public int  GetActiveBeatPosMs()        { return activeBeatPosMs ?? 0; }
    public int  GetActiveBeatEndPosMs()     { return activeBeatEndPosMs ?? 0; }

    // Half-beat getters
    public int  GetLastHalfBeat()               { return lastHalfBeat; }
    public int? GetActiveHalfBeat()             { return activeHalfBeat; }
    public int  GetActiveHalfBeatStartPosMs()   { return activeHalfBeatStartPosMs ?? 0; }
    public int  GetActiveHalfBeatPosMs()        { return activeHalfBeatPosMs ?? 0; }
    public int  GetActiveHalfBeatEndPosMs()     { return activeHalfBeatEndPosMs ?? 0; }
}