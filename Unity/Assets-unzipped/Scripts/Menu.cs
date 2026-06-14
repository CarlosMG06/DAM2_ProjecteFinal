using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject TitleScreenUI;
    [SerializeField] private GameObject LevelSelectUI;
    [SerializeField] private GameObject OptionsUI;
    [SerializeField] private GameObject LeaderboardUI;
    [SerializeField] private GameObject MusicSlider;
    [SerializeField] private TextMeshProUGUI MusicVolumeText;
    [SerializeField] private GameObject SFXSlider;
    [SerializeField] private TextMeshProUGUI SFXVolumeText;
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private ChartComposer chartComposer;
    private static bool shouldShowLevelSelect = false;

    public GameObject LevelSelectSongPrefab;
    public GameObject LeaderboardScorePrefab;
    public Transform LevelSelectSongParent;
    public Transform LeaderboardSongParent;
    public Transform LeaderboardScoreParent;

    public void ShowLeaderboardSongs() {
        print("ShowLeaderboardSongs");
        foreach (Transform child in LeaderboardSongParent)
            Destroy(child.gameObject);
        foreach ((int index, SongData song) in GlobalGameData.GetSongs().Select((song, index) => (index, song))) {
            print("ShowLeaderboardSongs: " + song.GetTitle());
            GameObject songObject = Instantiate(LevelSelectSongPrefab, LeaderboardSongParent);
            songObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = song.GetTitle();
            songObject.GetComponent<Button>().onClick.AddListener(() => ShowLeaderboardScores(index));

        }
    }

    public void ShowLeaderboardScores(int songIndex) {
        foreach (Transform child in LeaderboardScoreParent)
            Destroy(child.gameObject);
        foreach (LevelSaveData levelSaveData in GlobalGameData.GetLevels()[songIndex]) {
            GameObject scoreObject = Instantiate(LeaderboardScorePrefab, LeaderboardScoreParent);
            scoreObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = levelSaveData.GetPlayerName();
            scoreObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = levelSaveData.GetHighscore().ToString();
            scoreObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = levelSaveData.GetRank();
        }
    }

    public void ShowLevelSelectSongs() {
        foreach (Transform child in LevelSelectSongParent)
            Destroy(child.gameObject);
        foreach ((int index, SongData song) in GlobalGameData.GetSongs().Select((song, index) => (index, song))) {
            GameObject songObject = Instantiate(LevelSelectSongPrefab, LevelSelectSongParent);
            songObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = song.GetTitle();
            songObject.GetComponent<Button>().onClick.AddListener(() => LoadLevel(index));
        }
    }

    public static void SetShowLevelSelect(bool show)
    {
        shouldShowLevelSelect = show;
    }

    void Start()
    {
        if (shouldShowLevelSelect)
        {
            ToLevelSelect();
            shouldShowLevelSelect = false;
        }
        else
        {
            ToTitle();
        }
        chartComposer.DefineCharts();

        // Set volumes according to saved options
        float musicVolume = GlobalGameData.GetOptions().GetMusicVolume();
        float sfxVolume = GlobalGameData.GetOptions().GetSFXVolume();
        MusicSlider.GetComponent<Slider>().value = musicVolume * 20;
        SFXSlider.GetComponent<Slider>().value = sfxVolume * 20;
        MusicVolumeText.text = "Music: " + musicVolume * 100 + "%";
        SFXVolumeText.text = "SFX: " + sfxVolume * 100 + "%";

        // Play Main Menu music
        AudioClip mainMenuMusic = Resources.Load<AudioClip>("Audio/Music/Fruit Salad");
        audioPlayer.PlayMusic(mainMenuMusic);
    }

    private string GetRandomPlayerName()
    {
        return "Player " + Random.Range(1, 1000000);
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
        ShowLevelSelectSongs();
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
        ShowLeaderboardSongs();
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevel(int songIndex)
    {
        SongData songData = GlobalGameData.GetSongs()[songIndex];
        LevelRunData.SetSong(songData);
        LevelRunData.SetSongIndex(songIndex);
        
        SceneManager.LoadScene("Level");
    }

    public void SetMusicVolume()
    {
        float value = MusicSlider.GetComponent<Slider>().value;
        MusicVolumeText.text = "Music: " + value * 5 + "%";
        audioPlayer.SetMusicVolume(value/20);
    }
    public void SetSFXVolume()
    {
        float value = SFXSlider.GetComponent<Slider>().value;
        SFXVolumeText.text = "SFX: " + value * 5 + "%";
        audioPlayer.SetSFXVolume(value/20);
    }

}
