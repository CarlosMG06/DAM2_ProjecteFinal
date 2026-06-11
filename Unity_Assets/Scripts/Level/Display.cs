using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Display : MonoBehaviour
{
    [SerializeField] private GameObject missText;
    [SerializeField] private GameObject okText;
    [SerializeField] private GameObject greatText;
    [SerializeField] private GameObject perfectText;
    [SerializeField] private GameObject resultMissText;
    [SerializeField] private GameObject resultOkText;
    [SerializeField] private GameObject resultGreatText;
    [SerializeField] private GameObject resultPerfectText;
    private List<GameObject> ratingList;
    [SerializeField] private GameObject songPlayingUI;
    [SerializeField] private GameObject levelResultsUI;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI resultMaxComboText;
    [SerializeField] private GameObject SongProgressFill;
    [SerializeField] private GameObject YouIndicator;

    void Awake()
    {
        ratingList = new List<GameObject> { missText, okText, greatText, perfectText };
        foreach (var rating in ratingList)
        {
            rating.SetActive(false);
        }
        songPlayingUI.SetActive(false);
        levelResultsUI.SetActive(false);
    }

    public void OnLevelStart()
    {
        songPlayingUI.SetActive(true);
        scoreText.text = "0";
        comboText.gameObject.SetActive(false);
        StartCoroutine(ShowYouIndicator());
    }

    public IEnumerator ShowRating(int ratingIndex, int fadeDurationMs, int showDurationMs)
{
    GameObject ratingToShow = ratingList[ratingIndex];
    var tmp = ratingToShow.GetComponent<TextMeshProUGUI>();
    Color baseColor = tmp.color;

    float elapsedTime = 0f;
    float fadeDuration = fadeDurationMs / 1000f;
    float showDuration = showDurationMs / 1000f;
    ratingToShow.SetActive(true);
    while (elapsedTime < fadeDuration)
    {
        float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
        tmp.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    yield return new WaitForSeconds(showDuration);
    elapsedTime = 0f;
    while (elapsedTime < fadeDuration)
    {
        float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
        tmp.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    ratingToShow.SetActive(false);
}

    public void UpdateRatingCount(int ratingIndex, int newCount)
    {
        switch (ratingIndex)
        {
            case 0:
                resultMissText.GetComponent<TextMeshProUGUI>().text = newCount.ToString();
                break;
            case 1:
                resultOkText.GetComponent<TextMeshProUGUI>().text = newCount.ToString();
                break;
            case 2:
                resultGreatText.GetComponent<TextMeshProUGUI>().text = newCount.ToString();
                break;
            case 3:
                resultPerfectText.GetComponent<TextMeshProUGUI>().text = newCount.ToString();
                break;
        }
    }
    
    public IEnumerator ShowYouIndicator()
    {
         // Fade in and out
        float elapsedTime = 0f;
        float fadeDuration = 1f;
        float showDuration = 3f;
        YouIndicator.SetActive(true);
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            YouIndicator.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0f, 0f, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(showDuration);
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            YouIndicator.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0f, 0f, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        YouIndicator.SetActive(false);
    }
    
    public void UpdateScore() {
        int newScore = LevelRunData.GetScore();
        scoreText.text = newScore.ToString();
    }
    public void UpdateCombo()
    {
        int newCombo = LevelRunData.GetCombo();
        if (newCombo > 1)
        {
            comboText.text = "x" + newCombo;
            comboText.gameObject.SetActive(true);
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }
    public void UpdateSongProgress(int timePositionMs, int songLengthMs)
    {
        float progress = (float)timePositionMs / songLengthMs;
        SongProgressFill.GetComponent<RectTransform>().sizeDelta = 
            new Vector2(
                progress * 900f, 
                SongProgressFill.GetComponent<RectTransform>().sizeDelta.y
            );
    }

    public void ShowResults() {
        int score = LevelRunData.GetScore();
        songPlayingUI.SetActive(false);
        levelResultsUI.SetActive(true);
        resultScoreText.text = "Score: " + score;
        resultMaxComboText.text = "Max Combo: x" + LevelRunData.GetMaxCombo();
    }

    public void ToLevelSelect()
    {
        Menu.SetShowLevelSelect(true);
        SceneManager.LoadScene("Menu");
    }

    public void ResetScene()
    {
        Debug.Log("Reiniciant l'escena...");

        // Tornem a carregar l'escena activa
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
