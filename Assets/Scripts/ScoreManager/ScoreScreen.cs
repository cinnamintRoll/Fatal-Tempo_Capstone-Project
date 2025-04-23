using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreScreen : MonoBehaviour
{
    [SerializeField] private List<SongData> SongList;
    [SerializeField] private GameObject _ScoreScreen;
    [SerializeField] private GameObject _LevelSelect;
    [SerializeField] private LevelSelectManager _LevelSelectManager;
    private SongData _SelectedSong;

    [Header("Song Info UI")]
    [SerializeField] private Text songNameText;
    [SerializeField] private Image albumCoverImage;

    [Header("Sliders")]
    [SerializeField] private Slider killSlider;
    [SerializeField] private Slider collectibleSlider;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text killText;
    [SerializeField] private TMP_Text collectibleText;
    
    [Header("Animators")]
    [SerializeField] private Animator HighScoreText;
    [SerializeField] private Animator DiskUI;
    [SerializeField] private float ScoreAnimationDuration = 1.5f;
    private bool isNewHighScore = false;

    private void Start()
    {
        string songName = PlayerPrefs.GetString("SongName");

        if (!string.IsNullOrEmpty(songName))
        {
            _ScoreScreen.SetActive(true);
            _LevelSelect.SetActive(false);

            int playerScore = PlayerPrefs.GetInt("SongScore");
            int fullCombo = PlayerPrefs.GetInt("FullCombo");
            int totalHits = PlayerPrefs.GetInt("TotalHits");
            int bestCombo = PlayerPrefs.GetInt("BestCombo");
            int totalMaxPoints = PlayerPrefs.GetInt("TotalMaxPoints");
            int totalCollected = PlayerPrefs.GetInt("TotalCollected");

            StartCoroutine(ShowScoreScreen(playerScore, bestCombo, totalHits, fullCombo, totalCollected, totalMaxPoints, songName));
        }
    }

    private IEnumerator ShowScoreScreen(int playerScore, int bestCombo, int totalHits, int fullCombo, int totalCollected, int totalMaxPoints, string songName)
    {
        _SelectedSong = SongList.Find(song => song.songName == songName);
        yield return AnimateScoreAndGrade(playerScore, totalMaxPoints, ScoreAnimationDuration);

        songNameText.text = _SelectedSong.songName;
        albumCoverImage.sprite = _SelectedSong.AlbumCover;

        comboText.text = $"{bestCombo}";
        killText.text = $"0 {fullCombo}";
        yield return AnimateSlider(killSlider, killText, totalHits, 1f, fullCombo.ToString());

        collectibleText.text = $"0 {totalCollected}";
        yield return AnimateSlider(collectibleSlider, collectibleText, totalCollected, 1f, totalCollected.ToString());

        string finalGrade = CalculateGrade(playerScore, totalMaxPoints);

        
        if (_SelectedSong != null)
        {
            if (playerScore > _SelectedSong.playerScore)
            {
                _SelectedSong.playerScore = playerScore;
                _SelectedSong.letterGrade = finalGrade;
                _SelectedSong.HighestCombo = bestCombo;
                _SelectedSong.FullCombo = fullCombo;
                _SelectedSong.maxPoints = totalMaxPoints;

                HighScoreText.SetTrigger("NewHigh");
                Debug.Log($"New high score saved for {songName}!");
            }
            else
            {
                Debug.Log($"Previous high score for {songName} is higher. No save performed.");
            }
        }
        else
        {
            Debug.LogWarning("SongData not found for: " + songName);
        }
        PlayerPrefs.DeleteKey("SongName");
        PlayerPrefs.DeleteKey("SongScore");
        PlayerPrefs.DeleteKey("FullCombo");
        PlayerPrefs.DeleteKey("TotalHits");
        PlayerPrefs.DeleteKey("BestCombo");
        PlayerPrefs.DeleteKey("TotalMaxPoints");
        PlayerPrefs.DeleteKey("TotalCollected");
    }

    private IEnumerator AnimateScoreAndGrade(int finalScore, int maxScore, float duration)
    {
        float elapsed = 0f;
        int currentScore = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentScore = Mathf.RoundToInt(Mathf.Lerp(0, finalScore, t));
            scoreText.text = currentScore.ToString("D7");

            string grade = CalculateGrade(currentScore, maxScore);
            gradeText.text = grade;

            yield return null;
        }

        scoreText.text = finalScore.ToString("D7");
        gradeText.text = CalculateGrade(finalScore, maxScore);
    }

    private IEnumerator AnimateSlider(Slider slider, TMP_Text text, int finalValue, float duration, string suffix = "")
    {
        float elapsed = 0f;
        int currentValue = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentValue = Mathf.RoundToInt(Mathf.Lerp(0, finalValue, t));
            text.text = $"{currentValue} {suffix}".Trim();
            slider.value = (float)currentValue / slider.maxValue;
            yield return null;
        }

        text.text = $"{finalValue} {suffix}".Trim();
        slider.value = (float)finalValue / slider.maxValue;
    }


    private string CalculateGrade(int score, int maxScore)
    {
        if (maxScore == 0) return "F";

        float percent = (float)score / maxScore;
        if (percent >= 0.95f) return "SS";
        if (percent >= 0.90f) return "S";
        if (percent >= 0.80f) return "A";
        if (percent >= 0.70f) return "B";
        if (percent >= 0.60f) return "C";
        return "F";
    }

    public void ReturnToLevelSelect()
    {
        _ScoreScreen.SetActive(false);
        _LevelSelect.SetActive(true);
        if (_SelectedSong != null)
        {
            _LevelSelectManager.SelectSong(_SelectedSong);
        }
    }
}
