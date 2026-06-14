using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerInput : MonoBehaviour {

    public Judge judge;

    void Start()
    {
        judge = GetComponent<Judge>();
    }

    void Update()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame) {
            judge.OnLeftArrowPressed();
        } else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) {
            judge.OnRightArrowPressed();
        }
    }

}
