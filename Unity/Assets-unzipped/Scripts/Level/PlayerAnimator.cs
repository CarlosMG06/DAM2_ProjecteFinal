using UnityEngine;
using System.Collections;

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

/*
    // Nudge settings
    [SerializeField] private float nudgeDistance = 0.25f;
    [SerializeField] private float nudgeDurationMs = 200f;
*/

    // Base animation speed at 1 beat between inputs (bps/2)
    private float baseSpeedMultiplier;

    void Awake()
    {
        leftAnimator   = leftCharacter.GetComponent<Animator>();
        playerAnimator = playerCharacter.GetComponent<Animator>();
        rightAnimator  = rightCharacter.GetComponent<Animator>();
    }

    void OnLevelStart()
    {
        float bpm = LevelRunData.GetSong().GetBPM();
        float bps = bpm / 60f;
        baseSpeedMultiplier = bps / 2f;

        // Start with a neutral gap of 1 beat
        SetSpeedMultiplier(baseSpeedMultiplier);
    }

    // Called by Judge/Synchronizer after a successful or failed key input
    public void OnPlayerKeySuccess() { OnAnyKeyInput(); }
    public void OnPlayerKeyFailure() { OnAnyKeyInput(); }

    void OnAnyKeyInput()
    {
        
        // Recalculate speed based on gap to the next input
        UpdateAnimationSpeed();

        // Nudge all characters to the right
        /*StartCoroutine(NudgeRight(leftCharacter));
        StartCoroutine(NudgeRight(playerCharacter));
        StartCoroutine(NudgeRight(rightCharacter));*/
    }

    void UpdateAnimationSpeed()
    {
        var chart = LevelRunData.GetSong().GetChart();

        // Use the beat of the last processed input as the reference point.
        // Get from the chart the next input from "now" using the metronome's lastBeat.
        float lastBeat = Judge.metronome.GetLastBeat();
        var (nextInputBeat, nextKey) = chart.GetNextInput(lastBeat);

        if (nextInputBeat < 0 || nextKey == null)
        {
            // Song is over — freeze all characters
            SetSpeedMultiplier(0f);
            return;
        }

        // Gap in beats between the last input beat and the next one
        // lastBeat here is the whole-beat counter; the actual "last input beat"
        // is implicitly the one we just processed, which sat at or before lastBeat.
        float gap = nextInputBeat - lastBeat;

        if (gap > 1.5f)
        {
            // Too far apart — freeze
            SetSpeedMultiplier(0f);
            return;
        }

        // Inverse proportion: gap 0.5 → ×2, gap 1 → ×1, gap 1.5 → ×0.67
        float speedScale = (gap > 0f) ? (1f / gap) : 1f;
        SetSpeedMultiplier(baseSpeedMultiplier * speedScale);
    }

    void SetSpeedMultiplier(float value)
    {
        leftAnimator.SetFloat("speedMultiplier",   value);
        playerAnimator.SetFloat("speedMultiplier", value);
        rightAnimator.SetFloat("speedMultiplier",  value);
    }
/*
    // Smoothly nudge a character nudgeDistance units to the right over nudgeDurationMs
    IEnumerator NudgeRight(GameObject character)
    {
        Vector3 startPos = character.transform.localPosition;
        Vector3 endPos   = startPos + new Vector3(nudgeDistance, 0f, 0f);
        float elapsed    = 0f;
        float duration   = nudgeDurationMs / 1000f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease out: fast start, slows to a stop
            t = 1f - (1f - t) * (1f - t);
            character.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        character.transform.localPosition = endPos;
    }
    */

}