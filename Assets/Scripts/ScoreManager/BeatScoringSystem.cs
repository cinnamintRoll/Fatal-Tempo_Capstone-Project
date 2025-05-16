using UnityEngine;
using TMPro;

public class BeatScoringSystem : MonoBehaviour
{
    public bool useMusicManager = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;

    private float beatIntervalSec; // seconds per beat
    private float timeSinceLastPulse; 

    private int hitStreak = 0;
    private int totalScore = 0;
    private int bestCombo = 0;

    private int totalCollected = 0;
    private int totalHits = 0;
    private bool pulseReceived = false;
    // Scoring windows in milliseconds
    [SerializeField] private int perfectWindow = 100; // ms
    [SerializeField] private int maxWindow = 300;     // ms

    [SerializeField] private GameObject scorePopupPrefab;

    public int PerfectWindow => perfectWindow;
    public int MaxWindow => maxWindow;

    private void Start()
    {
        if (useMusicManager && MusicManager.Instance != null)
        {
            float bpm = MusicManager.Instance.bpm;
            beatIntervalSec = 60f / bpm;

            MusicManager.Instance.OnIntervalPassed.AddListener(OnPulse);
            timeSinceLastPulse = 0f; // initialize
        }

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnDamage.AddListener(OnMissOrHitByEnemy);
            PlayerHealth.Instance.OnKillEnemy.AddListener(OnHitEnemy);
        }

        UpdateScoreDisplay();
    }

    private void Update()
    {
        if (pulseReceived)
        {
            timeSinceLastPulse += Time.deltaTime;
        }
    }

    private void OnPulse()
    {
        Debug.Log("Pulsed");
        timeSinceLastPulse = 0f;
        pulseReceived = true;
    }

    public int GetHitScore()
    {
        if (!pulseReceived)
        {
            Debug.LogWarning("No pulse received yet, cannot score.");
            return 0;
        }

        float halfBeat = beatIntervalSec / 2f;

        float offset = timeSinceLastPulse;


        if (offset > halfBeat)
            offset -= beatIntervalSec;

        float offsetMS = offset * 1000f;
        float absOffsetMS = Mathf.Abs(offsetMS);

        Debug.Log($"Hit offset: {(offsetMS >= 0f ? "+" : "")}{offsetMS:F1} ms");

        if (absOffsetMS <= perfectWindow)
        {
            return 300;
        }
        else if (absOffsetMS <= maxWindow)
        {
            float t = (absOffsetMS - perfectWindow) / (maxWindow - perfectWindow);
            return Mathf.RoundToInt(Mathf.Lerp(300, 1, t));
        }
        else
        {
            return 0;
        }
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
        GameObject popup = Instantiate(scorePopupPrefab, position, Quaternion.identity);
        ScorePopup scorePopup = popup.GetComponent<ScorePopup>();
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

    public int GetTotalCollected() => totalCollected;
    public int GetTotalHits() => totalHits;
}
