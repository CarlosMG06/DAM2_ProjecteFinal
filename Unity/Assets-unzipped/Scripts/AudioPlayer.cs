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
    [SerializeField] private GameObject SFXPrefab;
    [SerializeField] private GameObject MusicPrefab;
    [SerializeField] private Transform Canvas;
    private AudioSource currentMusicSource;

    private int currentSongLengthMs;

    [SerializeField] private Display display;
    
    void Awake() {
        instance = this;
    }

    public void SetSFXVolume(float newVolume) {
        GlobalGameData.GetOptions().SetSFXVolume(newVolume);

        // Update any existing SFX GameObjects
        GameObject[] sfx = GameObject.FindGameObjectsWithTag("SFX");
        foreach (GameObject obj in sfx){
            obj.GetComponent<AudioSource>().volume = newVolume;
        }
    }

    public void SetMusicVolume(float newVolume) {
        GlobalGameData.GetOptions().SetMusicVolume(newVolume);

        // Update any existing music GameObjects
        GameObject[] music = GameObject.FindGameObjectsWithTag("Music");
        foreach (GameObject obj in music){
            obj.GetComponent<AudioSource>().volume = newVolume;
        }
    }

    public void PlaySound(AudioClip clip, float relativeVolume = 1f, string text = "", float pitch = 1f) {
        if (clip == null) 
            Debug.LogError("AudioClip is null");
        GameObject obj = Instantiate(instance.SFXPrefab);
        AudioSource source = obj.GetComponent<AudioSource>();
        source.clip = clip;
        source.volume = relativeVolume * GlobalGameData.GetOptions().GetSFXVolume();
        obj.transform.GetChild(0).GetComponent<Text>().text = text;
        source.pitch = pitch;
    } 

    public void PlayMusic(AudioClip clip, bool loop = true, bool broadcastEnd = false)
    {   
       GameObject obj = Instantiate(instance.MusicPrefab);
       AudioSource source = obj.GetComponent<AudioSource>();
       source.clip = clip;
       source.volume = GlobalGameData.GetOptions().GetMusicVolume();
       currentSongLengthMs = (int)(clip.length * 1000);
       if (loop)
        {
            source.loop = true;
        }
        source.Play();
        currentMusicSource = source;
        if (broadcastEnd)
        {
            StartCoroutine(BroadcastEndOfSong(source));
        }
    }

    private IEnumerator BroadcastEndOfSong(AudioSource source)
    {
        while (source.isPlaying)
        {
            yield return null;
        }
        BroadcastMessage("OnSongEnd", SendMessageOptions.DontRequireReceiver);
    }

    void OnLevelStart()
    {
        AudioClip song = LevelRunData.GetSong().GetAudio();
        PlayMusic(song, false, true);
        StartCoroutine(UpdateTimePosition());
    }

    void OnLevelEnd()
    {
        StopCoroutine(UpdateTimePosition());
    }
    
    IEnumerator UpdateTimePosition()
    {
        while (true)
        {
            float deltaTimeMs = Time.deltaTime * 1000f;
            float newTimePosition = LevelRunData.GetTimePositionMs() + deltaTimeMs;
            LevelRunData.SetTimePositionMs(newTimePosition); // store as float
            display.UpdateSongProgress(currentMusicSource);
            yield return null;
        }
    }

    public int GetCurrentSongLengthMs() {
        return currentSongLengthMs;
    }
}
