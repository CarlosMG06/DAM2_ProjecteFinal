using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour {
    void Awake() {
        // Destroy after playing if it doesn't loop
        //AudioSource audioSource = this.GetComponent<AudioSource>();
        //audioSource.Play();
        //print(audioSource == null);
        //if (audioSource != null && audioSource.clip != null && !audioSource.loop) {
        //    Destroy(this, audioSource.clip.length);
        //}
    }
}
