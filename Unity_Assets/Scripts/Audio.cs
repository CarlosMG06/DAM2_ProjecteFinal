using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour {
    void Awake() {
        // Destroy after playing
        Destroy(gameObject, GetComponent<AudioSource>().clip.length);
    }
}
