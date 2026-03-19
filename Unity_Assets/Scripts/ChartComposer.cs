using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public static class ChartComposer 
{
    /* 
        Defines charts: what keys have to be pressed on what beats
        then stores them in GlobalGameData
    */

    static List<SongChart> charts;
    static List<Dictionary<int, string>> requiredInputsList;
    
    static void Start()
    {        
        for (int i = 0; i < GlobalGameData.GetSongs().Count; i++) {
            requiredInputsList.Add(new Dictionary<int, string>());
            
        }
    }

}

public class SongChart {
    private Dictionary<int, Key> requiredInputs;
    private int lastInputBeat;

    public SongChart(Dictionary<int, Key> requiredInputs) {
        this.requiredInputs = requiredInputs;
        int maxBeat = 0;
        foreach (var beat in requiredInputs.Keys) {
            maxBeat = Math.Max(maxBeat, beat);
        }
        this.lastInputBeat = maxBeat;
    }

    public (int, Key?) GetNextInput(int lastBeat) {
        int nextInputBeat = lastBeat;
        while (!requiredInputs.ContainsKey(nextInputBeat)) {
            nextInputBeat += 1;
            if (nextInputBeat > lastInputBeat) {
                return (-1, null);
            }
        }
        Key nextInputKey = requiredInputs[nextInputBeat];
        return (nextInputBeat, nextInputKey);
    }
}