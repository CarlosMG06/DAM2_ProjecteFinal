using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInput : MonoBehaviour
{
    /* 
        Listens to the players input
        
        Broadcasts if any key in the controls is being pressed
    */

    void Update() {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame) {
            BroadcastMessage("OnLeftArrowPressed", SendMessageOptions.DontRequireReceiver);
        } else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) {
            BroadcastMessage("OnRightArrowPressed", SendMessageOptions.DontRequireReceiver);
        }
    }

}
