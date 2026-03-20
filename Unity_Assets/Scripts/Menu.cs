using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject TitleScreenUI;
    [SerializeField] private GameObject LevelSelectUI;
    [SerializeField] private GameObject OptionsUI;
    [SerializeField] private GameObject LeaderboardUI;

    void Start()
    {
        ToTitle();
    }
    public void ToTitle()
    {
        TitleScreenUI.SetActive(true);
        LevelSelectUI.SetActive(false);
        OptionsUI.SetActive(false);
        LeaderboardUI.SetActive(false);
    }
    public void ToLevelSelect()
    {
        TitleScreenUI.SetActive(false);
        LevelSelectUI.SetActive(true);
    }
    public void ToOptions()
    {
        TitleScreenUI.SetActive(false);
        OptionsUI.SetActive(true);
    } 
    public void ToLeaderboard()
    {
        TitleScreenUI.SetActive(false);
        LeaderboardUI.SetActive(true);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevel(string songTitle)
    {
        SongData songData = GlobalGameData.GetSongFromTitle(songTitle);
        LevelRunData.SetSong(songData);
        SceneManager.LoadScene("Level");
    }

}
