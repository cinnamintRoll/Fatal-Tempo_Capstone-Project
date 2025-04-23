using UnityEngine;
using TMPro;

public class BeatScoringSystem : MonoBehaviour
{
    public bool useMusicManager = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text multiplierText;

    private float beatIntervalMS;
    private float lastPulseTime;

    private int currentMultiplier = 1;
    private int hitStreak = 0;
    private int totalScore = 0;
    private int bestCombo = 0;

    private int totalCollected = 0;
    private int totalHits = 0;

    // Scoring windows
    [SerializeField] private int perfectWindow = 100; // in ms
    [SerializeField] private int maxWindow = 300;    // in ms

    public int PerfectWindow => perfectWindow;
    public int MaxWindow => maxWindow;

    private void Start()
    {
        if (useMusicManager && MusicManager.Instance != null)
        {
            float bpm = MusicManager.Instance.bpm;
            beatIntervalMS = 60000f / bpm;

            MusicManager.Instance.OnIntervalPassed.AddListener(OnPulse);
        }

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnDamage.AddListener(OnMissOrHitByEnemy);
            PlayerHealth.Instance.OnKillEnemy.AddListener(OnHitEnemy);
        }

        UpdateScoreDisplay();
    }

    private void OnPulse()
    {
        lastPulseTime = Time.time;
    }

    public int GetHitScore()
    {
        float hitTime = Time.time;
        float currentIntervalSec = beatIntervalMS / 1000f;
        float timeSinceLastPulse = hitTime - lastPulseTime;

        // Calculate signed offset to the nearest beat
        float rawOffset = timeSinceLastPulse % currentIntervalSec;
        if (rawOffset > currentIntervalSec / 2f)
            rawOffset -= currentIntervalSec;

        float distanceMS = rawOffset * 1000f;

        // Debug info: Show early/late timing with +/- sign
        Debug.Log($"Hit offset: {(distanceMS >= 0f ? "+" : "")}{distanceMS:F1} ms");

        float absDistanceMS = Mathf.Abs(distanceMS);

        if (absDistanceMS <= perfectWindow)
            return 300;

        // Linear falloff: beyond perfectWindow, score decreases toward 0
        float t = (absDistanceMS - perfectWindow) / perfectWindow; // falloff rate
        t = Mathf.Clamp01(t); // keep score from going negative
        return Mathf.RoundToInt(Mathf.Lerp(300, 100, t));
    }


    public void OnHitEnemy()
    {
        int baseScore = GetHitScore();
        UpdateMultiplier(true);
        int scoreToAdd = baseScore * currentMultiplier;

        totalScore += scoreToAdd;
        totalHits++;

        Debug.Log($"Scored: {baseScore} x{currentMultiplier} = {scoreToAdd}");

        UpdateScoreDisplay();
    }

    public void OnMissOrHitByEnemy()
    {
        //ResetMultiplier();
        Debug.Log("Missed! Multiplier reset.");
        UpdateScoreDisplay();
    }

    public void OnItemCollected(int itemValue)
    {
        totalScore += itemValue;
        totalCollected++;

        Debug.Log($"Item Collected! Total Score: {totalScore}");

        UpdateScoreDisplay();
    }

    private void UpdateMultiplier(bool successfulHit)
    {
        if (successfulHit)
        {
            hitStreak++;

            if (hitStreak > bestCombo)
            {
                bestCombo = hitStreak;
            }
            /*
            if (hitStreak == 4)
                currentMultiplier = 8;
            else if (hitStreak == 3)
                currentMultiplier = 4;
            else if (hitStreak == 2)
                currentMultiplier = 2;
            else
                currentMultiplier = 1;
            */
        }
    }

    private void ResetMultiplier()
    {
        hitStreak = 0;
        currentMultiplier = 1;
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"{totalScore.ToString("D7")}";

        if (multiplierText != null)
            multiplierText.text = $"{currentMultiplier}x";
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt("SongScore", totalScore);
        PlayerPrefs.SetInt("TotalCollected", totalCollected);
        PlayerPrefs.SetInt("TotalHits", totalHits);
        PlayerPrefs.SetInt("BestCombo", bestCombo);
    }

    // Optional getters
    public int GetTotalCollected() => totalCollected;
    public int GetTotalHits() => totalHits;
}
