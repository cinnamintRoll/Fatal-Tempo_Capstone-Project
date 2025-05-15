using UnityEngine;
using TMPro;

public class BeatScoringSystem : MonoBehaviour
{
    public bool useMusicManager = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;

    private float beatIntervalSec; // seconds per beat
    private float lastPulseMusicTime; // time of last beat in music time (AudioSource.time)

    private int hitStreak = 0;
    private int totalScore = 0;
    private int bestCombo = 0;

    private int totalCollected = 0;
    private int totalHits = 0;

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
            lastPulseMusicTime = 0f; // initialize
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
        // Capture the precise music time at the beat interval
        if (MusicManager.Instance != null && MusicManager.Instance.musicSource != null)
        {
            lastPulseMusicTime = MusicManager.Instance.musicSource.time;
        }
    }

    public int GetHitScore()
    {
        if (MusicManager.Instance == null || MusicManager.Instance.musicSource == null)
        {
            Debug.LogWarning("MusicManager or AudioSource missing.");
            return 0;
        }

        // Get current music playback time
        float currentMusicTime = MusicManager.Instance.musicSource.time;

        // Calculate offset from last pulse in seconds
        float timeSinceLastPulse = currentMusicTime - lastPulseMusicTime;

        // Wrap offset so it lies within [-beatIntervalSec/2, +beatIntervalSec/2]
        float halfBeat = beatIntervalSec / 2f;
        float offset = timeSinceLastPulse;

        if (offset > halfBeat)
            offset -= beatIntervalSec;
        else if (offset < -halfBeat)
            offset += beatIntervalSec;

        float offsetMS = offset * 1000f; // convert to milliseconds
        float absOffsetMS = Mathf.Abs(offsetMS);

        Debug.Log($"Hit offset: {(offsetMS >= 0f ? "+" : "")}{offsetMS:F1} ms");

        // Scoring logic: 300 for perfect (<= perfectWindow), linearly down to 100 at maxWindow
        if (absOffsetMS <= perfectWindow)
        {
            return 300;
        }
        else if (absOffsetMS <= maxWindow)
        {
            // Linear interpolation from 300 down to 100 between perfectWindow and maxWindow
            float t = (absOffsetMS - perfectWindow) / (maxWindow - perfectWindow);
            return Mathf.RoundToInt(Mathf.Lerp(300, 100, t));
        }
        else
        {
            return 0; // Missed the timing window
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
