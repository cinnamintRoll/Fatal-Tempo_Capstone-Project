using BNG;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform baseTransform;
    [SerializeField] private BeatScoringSystem scoringSystem;
    [SerializeField] private ScreenFader screenFader;

    public Transform PlayerTransform
    {
        get
        {
            if (playerTransform == null)
            {
                Debug.LogWarning("Player Transform is not set!");
            }
            return playerTransform;
        }
    }

    public Transform BaseTransform
    {
        get
        {
            if (baseTransform == null)
            {
                Debug.LogWarning("Base Transform is not set!");
            }
            return baseTransform;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public void SetBaseTransform(Transform transform)
    {
        baseTransform = transform;
    }

    public int GetTotalCombo()
    {
        int totalEnemies = EnemyTracker.Instance != null ? EnemyTracker.Instance.GetTotalEnemyCount() : 0;
        int totalCollectibles = CollectibleTracker.Instance != null ? CollectibleTracker.Instance.GetTotalCollectibleCount() : 0;
        return totalEnemies + totalCollectibles;
    }

    public int GetTotalSongScore()
    {
        int totalEnemies = EnemyTracker.Instance != null ? EnemyTracker.Instance.GetTotalEnemyCount() : 0;
        int maxKillpoints = scoringSystem.MaxWindow;
        return totalEnemies * maxKillpoints;
    }

    public void SaveSongScore()
    {
        if (MusicManager.Instance != null)
        {
            string songName = MusicManager.Instance.song.songName;
            PlayerPrefs.SetString("SongName", songName);
        }

        PlayerPrefs.SetInt("TotalMaxPoints", GetTotalSongScore());
        PlayerPrefs.SetInt("FullCombo", GetTotalCombo());
        scoringSystem.SaveScore();
    }

    public int GetTotalEnemiesKilled()
    {
        return EnemyTracker.Instance != null ? EnemyTracker.Instance.GetTotalEnemiesKilled() : 0;
    }

    public int GetTotalCollectiblesCollected()
    {
        return CollectibleTracker.Instance != null ? CollectibleTracker.Instance.GetTotalCollectiblesCollected() : 0;
    }

    public ScreenFader GetScreenFader()
    {
        return screenFader;
    }
}
