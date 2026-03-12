using UnityEngine;

public class Metronome : MonoBehaviour
{
    /*
        Keeps track of the beat of the song
    */

    private int bpm;
    private int offsetMs;
    private const int marginMs = 80;
    private int beatDurationMs;
    private int lastBeat;
    private int nextBeatPosMs;
    private int? activeBeat;
    private int nextBeatStartPosMs;
    private int activeBeatEndPosMs;

    void StartLevel()
    {
        bpm = LevelRunData.GetSong().GetBPM();
        offsetMs = LevelRunData.GetSong().GetOffsetMs();
        beatDurationMs = 60 / bpm * 1000;
        lastBeat = 0;
        nextBeatPosMs = offsetMs + beatDurationMs;
        nextBeatStartPosMs = nextBeatPosMs - marginMs;
        activeBeatEndPosMs = nextBeatPosMs + marginMs;
        activeBeat = null;
    }

    void Update()
    {
        if (LevelRunData.GetIsActive()) {
            int timePositionMs = LevelRunData.GetTimePositionMs();
            if (timePositionMs >= nextBeatStartPosMs) {
                activeBeat = lastBeat + 1;
                BroadcastMessage("EnterBeat", activeBeat);
                nextBeatStartPosMs += beatDurationMs;
            }
            if (timePositionMs >= nextBeatPosMs) {
                BroadcastMessage("Beat", activeBeat);
                nextBeatPosMs += beatDurationMs;
            }
            if (timePositionMs >= activeBeatEndPosMs) {
                lastBeat += 1;
                activeBeat = null;
                BroadcastMessage("ExitBeat", activeBeat);
                activeBeatEndPosMs += beatDurationMs;
            }
        }
    }

    public int GetLastBeat() { return lastBeat; }
    public int? GetActiveBeat() { return activeBeat; }
    

    // void StartLevel() {

    // }

    // void EndLevel() {

    // }
}
