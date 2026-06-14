using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChartComposer : MonoBehaviour
{
    /* 
        Defines charts: what keys have to be pressed on what beats
        then stores them in GlobalGameData
    */

    public void DefineCharts()
    {        
        for (int i = 0; i < GlobalGameData.GetSongs().Count; i++) {
            Dictionary<float, Key> requiredInputs = new Dictionary<float, Key>();
            if (i == 0) {
                // required inputs: every beat from 9 to 117
                // additional half-beats at 15.5, 23.5, 33.5, 37.5, 47.5 & 48.5 (remove 47), 
                // 55.5, 63.5 & 64.5 (remove 63), 67.5 & 68.5 (remove 67), 71.5, 81.5, 
                // 87.5 & 88.5 (remove 87), 95.5 & 96.5 (remove 95), 101.5, 107.5, 109.5 & 110.5 (remove 109), 
                // 113.5 & 114.5 (remove 113), 115.5
                // each key press alternates between left and right
                requiredInputs.Add(9, Key.LeftArrow);
                requiredInputs.Add(10, Key.RightArrow);
                requiredInputs.Add(11, Key.LeftArrow);
                requiredInputs.Add(12, Key.RightArrow);
                requiredInputs.Add(13, Key.LeftArrow);
                requiredInputs.Add(14, Key.RightArrow);
                requiredInputs.Add(15, Key.LeftArrow);
                requiredInputs.Add(15.5f, Key.RightArrow);
                requiredInputs.Add(16, Key.LeftArrow);
                requiredInputs.Add(17, Key.RightArrow);
                requiredInputs.Add(18, Key.LeftArrow);
                requiredInputs.Add(19, Key.RightArrow);
                requiredInputs.Add(20, Key.LeftArrow);
                requiredInputs.Add(21, Key.RightArrow);
                requiredInputs.Add(22, Key.LeftArrow);
                requiredInputs.Add(23, Key.RightArrow);
                requiredInputs.Add(23.5f, Key.LeftArrow);
                requiredInputs.Add(24, Key.RightArrow);
                requiredInputs.Add(25, Key.LeftArrow);
                requiredInputs.Add(26, Key.RightArrow);
                requiredInputs.Add(27, Key.LeftArrow);
                requiredInputs.Add(28, Key.RightArrow);
                requiredInputs.Add(29, Key.LeftArrow);
                requiredInputs.Add(30, Key.RightArrow);
                requiredInputs.Add(31, Key.LeftArrow);
                requiredInputs.Add(32, Key.RightArrow);
                requiredInputs.Add(33, Key.LeftArrow);
                requiredInputs.Add(33.5f, Key.RightArrow);
                requiredInputs.Add(34, Key.LeftArrow);
                requiredInputs.Add(35, Key.RightArrow);
                requiredInputs.Add(36, Key.LeftArrow);
                requiredInputs.Add(37, Key.RightArrow);
                requiredInputs.Add(37.5f, Key.LeftArrow);
                requiredInputs.Add(38, Key.RightArrow);
                requiredInputs.Add(39, Key.LeftArrow);
                requiredInputs.Add(40, Key.RightArrow);
                requiredInputs.Add(41, Key.LeftArrow);
                requiredInputs.Add(42, Key.RightArrow);
                requiredInputs.Add(43, Key.LeftArrow);
                requiredInputs.Add(44, Key.RightArrow);
                requiredInputs.Add(45, Key.LeftArrow);
                requiredInputs.Add(46, Key.RightArrow);
                requiredInputs.Add(47.5f, Key.LeftArrow);
                requiredInputs.Add(48, Key.RightArrow);
                requiredInputs.Add(48.5f, Key.LeftArrow);
                requiredInputs.Add(49, Key.RightArrow);
                requiredInputs.Add(50, Key.LeftArrow);
                requiredInputs.Add(51, Key.RightArrow);
                requiredInputs.Add(52, Key.LeftArrow);
                requiredInputs.Add(53, Key.RightArrow);
                requiredInputs.Add(54, Key.LeftArrow);
                requiredInputs.Add(55, Key.RightArrow);
                requiredInputs.Add(55.5f, Key.LeftArrow);
                requiredInputs.Add(56, Key.RightArrow);
                requiredInputs.Add(57, Key.LeftArrow);
                requiredInputs.Add(58, Key.RightArrow);
                requiredInputs.Add(59, Key.LeftArrow);
                requiredInputs.Add(60, Key.RightArrow);
                requiredInputs.Add(61, Key.LeftArrow);
                requiredInputs.Add(62, Key.RightArrow);
                requiredInputs.Add(63.5f, Key.LeftArrow);
                requiredInputs.Add(64, Key.RightArrow);
                requiredInputs.Add(64.5f, Key.LeftArrow);
                requiredInputs.Add(65, Key.RightArrow);
                requiredInputs.Add(66, Key.LeftArrow);
                requiredInputs.Add(67.5f, Key.RightArrow);
                requiredInputs.Add(68, Key.LeftArrow);
                requiredInputs.Add(68.5f, Key.RightArrow);
                requiredInputs.Add(69, Key.LeftArrow);
                requiredInputs.Add(70, Key.RightArrow);
                requiredInputs.Add(71, Key.LeftArrow);
                requiredInputs.Add(71.5f, Key.RightArrow);
                requiredInputs.Add(72, Key.LeftArrow);
                requiredInputs.Add(73, Key.RightArrow);
                requiredInputs.Add(74, Key.LeftArrow);
                requiredInputs.Add(75, Key.RightArrow);
                requiredInputs.Add(76, Key.LeftArrow);
                requiredInputs.Add(77, Key.RightArrow);
                requiredInputs.Add(78, Key.LeftArrow);
                requiredInputs.Add(79, Key.RightArrow);
                requiredInputs.Add(80, Key.LeftArrow);
                requiredInputs.Add(81, Key.RightArrow);
                requiredInputs.Add(81.5f, Key.LeftArrow);
                requiredInputs.Add(82, Key.RightArrow);
                requiredInputs.Add(83, Key.LeftArrow);
                requiredInputs.Add(84, Key.RightArrow);
                requiredInputs.Add(85, Key.LeftArrow);
                requiredInputs.Add(86, Key.RightArrow);
                requiredInputs.Add(87.5f, Key.LeftArrow);
                requiredInputs.Add(88, Key.RightArrow);
                requiredInputs.Add(88.5f, Key.LeftArrow);
                requiredInputs.Add(89, Key.RightArrow);
                requiredInputs.Add(90, Key.LeftArrow);
                requiredInputs.Add(91, Key.RightArrow);
                requiredInputs.Add(92, Key.LeftArrow);
                requiredInputs.Add(93, Key.RightArrow);
                requiredInputs.Add(94, Key.LeftArrow);
                requiredInputs.Add(95.5f, Key.RightArrow);
                requiredInputs.Add(96, Key.LeftArrow);
                requiredInputs.Add(96.5f, Key.RightArrow);
                requiredInputs.Add(97, Key.LeftArrow);
                requiredInputs.Add(98, Key.RightArrow);
                requiredInputs.Add(99, Key.LeftArrow);
                requiredInputs.Add(100, Key.RightArrow);
                requiredInputs.Add(101, Key.LeftArrow);
                requiredInputs.Add(101.5f, Key.RightArrow);
                requiredInputs.Add(102, Key.LeftArrow);
                requiredInputs.Add(103, Key.RightArrow);
                requiredInputs.Add(104, Key.LeftArrow);
                requiredInputs.Add(105, Key.RightArrow);
                requiredInputs.Add(106, Key.LeftArrow);
                requiredInputs.Add(107, Key.RightArrow);
                requiredInputs.Add(107.5f, Key.LeftArrow);
                requiredInputs.Add(108, Key.RightArrow);
                requiredInputs.Add(109.5f, Key.LeftArrow);
                requiredInputs.Add(110, Key.RightArrow);
                requiredInputs.Add(110.5f, Key.LeftArrow);
                requiredInputs.Add(111, Key.RightArrow);
                requiredInputs.Add(112, Key.LeftArrow);
                requiredInputs.Add(113.5f, Key.RightArrow);
                requiredInputs.Add(114, Key.LeftArrow);
                requiredInputs.Add(114.5f, Key.RightArrow);
                requiredInputs.Add(115, Key.LeftArrow);
                requiredInputs.Add(115.5f, Key.RightArrow);
                requiredInputs.Add(116, Key.LeftArrow);
                requiredInputs.Add(117, Key.RightArrow);
            } else if (i == 1) {
                requiredInputs.Add(1, Key.LeftArrow);
                requiredInputs.Add(2, Key.RightArrow);
                requiredInputs.Add(3, Key.LeftArrow);
                requiredInputs.Add(4, Key.RightArrow);
                requiredInputs.Add(5, Key.LeftArrow);
                requiredInputs.Add(6, Key.RightArrow);
                requiredInputs.Add(7, Key.LeftArrow);
                requiredInputs.Add(8, Key.RightArrow);
            }
            SongChart chart = new SongChart(requiredInputs);
            
            GlobalGameData.GetSongs()[i].SetChart(chart);
            
        }
    }

}

public class SongChart {
    private Dictionary<float, Key> requiredInputs;
    private float lastInputBeat;

    public SongChart(Dictionary<float, Key> requiredInputs) {
        this.requiredInputs = requiredInputs;
        float maxBeat = 0;
        foreach (var beat in requiredInputs.Keys) {
            maxBeat = Math.Max(maxBeat, beat);
        }
        this.lastInputBeat = maxBeat;
    }

    public (float, Key?) GetNextInput(float lastBeat) {
        float nextInputBeat = lastBeat + 0.5f;
        while (!requiredInputs.ContainsKey(nextInputBeat)) {
            nextInputBeat += 0.5f;
            if (nextInputBeat > lastInputBeat) {
                return (-1, null);
            }
        }
        Key nextInputKey = requiredInputs[nextInputBeat];
        return (nextInputBeat, nextInputKey);
    }
}