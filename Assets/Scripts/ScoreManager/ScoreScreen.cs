using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreScreen : MonoBehaviour
{
    [SerializeField] private List<SongData> SongList;
    [SerializeField] private GameObject _ScoreScreen;
    [SerializeField] private GameObject _LevelSelect;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text killText;
    [SerializeField] private TMP_Text collectibleText;
    [SerializeField] private TMP_Text highScoreText;

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

            string grade = CalculateGrade(playerScore, totalMaxPoints);

            // Update UI
            scoreText.text = $"Score: {playerScore}";
            gradeText.text = $"Grade: {grade}";
            comboText.text = $"Best Combo: {bestCombo}";
            killText.text = $"Kills: {totalHits}/{fullCombo}";
            collectibleText.text = $"Collected: {totalCollected}";
            highScoreText.text = ""; // Clear initially

            // Save only if this run is better
            SongData data = SongList.Find(song => song.songName == songName);
            if (data != null)
            {
                if (playerScore > data.playerScore)
                {
                    data.playerScore = playerScore;
                    data.letterGrade = grade;
                    data.HighestCombo = bestCombo;
                    data.FullCombo = fullCombo;
                    data.maxPoints = totalMaxPoints;

                    highScoreText.text = "New High Score!";
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
        }
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
}
