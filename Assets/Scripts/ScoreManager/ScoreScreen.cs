using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreScreen : MonoBehaviour
{
    //[SerializeField] private List<SongData> SongList;
    [SerializeField] private GameObject _ScoreScreen;
    [SerializeField] private GameObject _LevelSelect;
    [SerializeField] private LevelSelectManager _LevelSelectManager;
    [SerializeField] private RotatingLevelSelector _RotationSelector;
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
    [SerializeField] private TMP_Text caloriesText;
    [SerializeField] private AudioClip ScoreScreenClip;
    [Header("Animators")]
    [SerializeField] private Animator HighScoreText;
    [SerializeField] private Animator DiskUI;
    [SerializeField] private float ScoreAnimationDuration = 1.5f;
    private bool isNewHighScore = false;

    private void OnEnable()
    {
        string songName = PlayerPrefs.GetString("SongName");

        if (!string.IsNullOrEmpty(songName))
        {
            _LevelSelect.SetActive(false);
            _ScoreScreen.SetActive(true);

            int playerScore = PlayerPrefs.GetInt("SongScore");
            int fullCombo = PlayerPrefs.GetInt("FullCombo");
            int totalHits = PlayerPrefs.GetInt("TotalHits");
            int bestCombo = PlayerPrefs.GetInt("BestCombo");
            int totalMaxPoints = PlayerPrefs.GetInt("TotalMaxPoints");

            int totalCollected = PlayerPrefs.GetInt("TotalCollected");
            int maxCollected = PlayerPrefs.GetInt("MaxCollected");

            int enemiesKilled = PlayerPrefs.GetInt("EnemiesKilled");
            int totalEnemies = PlayerPrefs.GetInt("TotalEnemies");

            AudioSource levelselectsource = _LevelSelectManager.audioSource;
            if (ScoreScreenClip != null)
            {
                levelselectsource.clip = ScoreScreenClip;
            }
            else
            {
                levelselectsource.clip = AlbumDatabase.Instance.GetSongFromAlbums(songName).songClip;
            }
            levelselectsource.Play();

            StartCoroutine(ShowScoreScreen(playerScore, bestCombo, totalHits, fullCombo, totalCollected, maxCollected, enemiesKilled, totalEnemies, totalMaxPoints, songName));
        }
    }

    private IEnumerator ShowScoreScreen(
    int playerScore,
    int bestCombo,
    int totalHits,
    int fullCombo,
    int totalCollected,
    int maxCollected,
    int enemiesKilled,
    int totalEnemies,
    int totalMaxPoints,
    string songName)
    {
        _SelectedSong = AlbumDatabase.Instance.GetSongFromAlbums(songName);

        songNameText.text = _SelectedSong.songName;
        albumCoverImage.sprite = _SelectedSong.AlbumCover;

        

        // Enemy kills
        killText.text = $"0 {totalEnemies}";
        yield return AnimateSlider(killSlider, killText, enemiesKilled, 1f, totalEnemies.ToString());

        // Collectibles
        collectibleText.text = $"0 {maxCollected}";
        yield return AnimateSlider(collectibleSlider, collectibleText, totalCollected, 1f, maxCollected.ToString());
        yield return AnimateScoreAndGrade(playerScore, totalMaxPoints, ScoreAnimationDuration);
        yield return AnimateNumber(bestCombo, 1.5f, comboText);
        yield return AnimateNumber(CalorieTrackerManager.Instance.GetCalories(), 1.5f, caloriesText);
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

        // Clean up PlayerPrefs
        PlayerPrefs.DeleteKey("SongName");
        PlayerPrefs.DeleteKey("SongScore");
        PlayerPrefs.DeleteKey("FullCombo");
        PlayerPrefs.DeleteKey("TotalHits");
        PlayerPrefs.DeleteKey("BestCombo");
        PlayerPrefs.DeleteKey("TotalMaxPoints");
        PlayerPrefs.DeleteKey("TotalCollected");
        PlayerPrefs.DeleteKey("MaxCollected");
        PlayerPrefs.DeleteKey("EnemiesKilled");
        PlayerPrefs.DeleteKey("TotalEnemies");

        CalorieTrackerManager.Instance.ResetCalories();
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

    private IEnumerator AnimateNumber(float finalScore, float duration, TMP_Text Output)
    {
        float elapsed = 0f;
        float currentScore = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentScore = Mathf.Round(Mathf.Lerp(0, finalScore, t));

            Output.text = currentScore.ToString();

            yield return null;
        }

        Output.text = Mathf.Round(finalScore).ToString();
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
        if (percent >= 0.90f) return "SS";
        if (percent >= 0.80f) return "S";
        if (percent >= 0.70f) return "A";
        if (percent >= 0.60f) return "B";
        if (percent >= 0.50f) return "C";
        return "F";
    }

    public void ReturnToLevelSelect()
    {
        _ScoreScreen.SetActive(false);
        _LevelSelect.SetActive(true);
        if (_SelectedSong != null)
        {
            _RotationSelector.SelectAlbumSong(_SelectedSong);
        }
    }
}
