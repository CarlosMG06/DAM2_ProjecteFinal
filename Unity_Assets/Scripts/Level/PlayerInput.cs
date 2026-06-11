using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerInput : MonoBehaviour
{
    /* 
        Listens to the players input
        
        Broadcasts if any key in the controls is being pressed
    */

    void OnLevelStart()
    {
        StartCoroutine(ListenForInput());
    }
    void OnLevelEnd()
    {
        StopCoroutine(ListenForInput());
    }

    private IEnumerator ListenForInput() 
    {
        while (true) {
            yield return null;
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) {
                // debug
                print("Left arrow key pressed");
                BroadcastMessage("OnLeftArrowPressed");
            } else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) {
                // debug
                print("Right arrow key pressed");
                BroadcastMessage("OnRightArrowPressed");
            }
        }
    }

}
