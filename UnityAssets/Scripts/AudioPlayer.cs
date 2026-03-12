using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioPlayer : MonoBehaviour
{
    /*
        Plays songs and sound effects
        
        Keeps track of how much time has passed since a song has started
    */

    private static AudioPlayer instance;
    private GameObject SoundPrefab;
    private Transform Canvas;

    private SongData currentSong;
    
    void Awake() {
        instance = this;
        // OptionsData optionsData = SaveFile.Load<OptionsData>("OptionsData.Dat");
        // if (optionsData != null) {
        //     SetVolume(optionsData.volume);
        // }
    }

    public static void PlaySound(AudioClip clip, float relativeVolume = 1f, string text = "", float pitch = 1f) {
        if (clip == null) 
            Debug.LogError("AudioClip is null");
        GameObject obj = Instantiate(instance.SoundPrefab, instance.Canvas);
        AudioSource source = obj.GetComponent<AudioSource>();
        source.clip = clip;
        source.volume = relativeVolume * GlobalGameData.GetOptions().GetSFXVolume();
        obj.transform.GetChild(0).GetComponent<Text>().text = text;
        source.pitch = pitch;
    } 

    public static void PlayMusic() {

    }

    public static void SetSFXVolume(float newVolume) {
        GlobalGameData.GetOptions().SetSFXVolume(newVolume);

        // Update any existing SFX GameObjects
        GameObject[] sfx = GameObject.FindGameObjectsWithTag("SFX");
        foreach (GameObject obj in sfx){
            obj.GetComponent<AudioSource>().volume = newVolume*0.5f;
        }
    }

    public static void SetMusicVolume(float newVolume) {
        GlobalGameData.GetOptions().SetMusicVolume(newVolume);

        // Update any existing music GameObjects
        GameObject[] music = GameObject.FindGameObjectsWithTag("Music");
        foreach (GameObject obj in music){
            obj.GetComponent<AudioSource>().volume = newVolume*0.5f;
        }
    }

    void Update() {
        if (LevelRunData.GetIsActive())
            updateTimePosition();
    }

    private void updateTimePosition() {

    }

    void levelStart(int levelNumber) {
        // TODO: start playing music
    }

    void levelEnd() {
        // TODO: stop playing music
    }

}
