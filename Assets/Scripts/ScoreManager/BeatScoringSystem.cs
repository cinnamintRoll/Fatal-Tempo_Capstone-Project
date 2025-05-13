using UnityEngine;
using TMPro;
using static UnityEditor.FilePathAttribute;

public class BeatScoringSystem : MonoBehaviour
{
    public bool useMusicManager = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;

    private float beatIntervalMS;
    private float lastPulseTime;

    private int hitStreak = 0;
    private int totalScore = 0;
    private int bestCombo = 0;

    private int totalCollected = 0;
    private int totalHits = 0;

    // Scoring windows
    [SerializeField] private int perfectWindow = 100; // in ms
    [SerializeField] private int maxWindow = 300;    // in ms

    [SerializeField] private GameObject scorePopupPrefab;
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
        float t = (absDistanceMS - perfectWindow) / perfectWindow;
        t = Mathf.Clamp01(t);
        return Mathf.RoundToInt(Mathf.Lerp(300, 100, t));
    }

    public void OnHitEnemy(Vector3 enemy)
    {
        int baseScore = GetHitScore();
        totalScore += baseScore;
        totalHits++;

        hitStreak++;
        if (hitStreak > bestCombo)
        {
            bestCombo = hitStreak;
        }

        Debug.Log($"Scored: {baseScore} on {enemy}");

        Vector3 popupPosition = enemy + Vector3.up * 1.5f;
        SpawnScorePopup(popupPosition, baseScore);

        UpdateScoreDisplay();
    }
    public void OnHitEnemy()
    {
        int baseScore = GetHitScore();
        totalScore += baseScore;
        totalHits++;

        hitStreak++;
        if (hitStreak > bestCombo)
        {
            bestCombo = hitStreak;
        }

        UpdateScoreDisplay();
    }

    public void OnMissOrHitByEnemy()
    {
        hitStreak = 0;
        Debug.Log("Missed! Hit streak reset.");
        UpdateScoreDisplay();
    }

    public void OnItemCollected(int itemValue)
    {
        totalScore += itemValue;
        totalCollected++;

        Debug.Log($"Item Collected! Total Score: {totalScore}");

        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"{totalScore.ToString("D7")}";
    }

    public void SpawnScorePopup(Vector3 position, int score)
    {
        GameObject popup = Instantiate(scorePopupPrefab, transform.forward, Quaternion.identity);
        ScorePopup scorePopup = popup.GetComponent<ScorePopup>();
        Debug.DrawRay(position, popup.transform.position, Color.red, 2f);

        if (scorePopup != null)
        {
            scorePopup.setlocation(position);
            scorePopup.SetText(score.ToString());
        }
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
