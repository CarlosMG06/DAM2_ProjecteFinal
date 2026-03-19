using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Display : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI failText;
    [SerializeField] private TextMeshProUGUI okText;
    [SerializeField] private TextMeshProUGUI greatText;
    [SerializeField] private TextMeshProUGUI perfectText;
    private List<TextMeshProUGUI> ratingList;
    [SerializeField] private GameObject chooseDifficultyUI;
    [SerializeField] private GameObject levelResultsUI;

    void Awake()
    {
        ratingList = new List<TextMeshProUGUI> { failText, okText, greatText, perfectText };
        foreach (var rating in ratingList)
        {
            rating.gameObject.SetActive(false);
        }
        chooseDifficultyUI.SetActive(true);
        levelResultsUI.SetActive(false);
    }

    public IEnumerator ShowRating(int ratingIndex, int showDurationMs)
    {
        GameObject ratingToShow = ratingList[ratingIndex].gameObject;
        ratingToShow.SetActive(true);
        yield return new WaitForSecondsRealtime(showDurationMs/1000);
        ratingToShow.SetActive(false);
    }

    public void ShowResults() {
        levelResultsUI.SetActive(true);
    }

    public void ResetScene()
    {
        Debug.Log("Reiniciant l'escena...");

        // Tornem a carregar l'escena activa
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
