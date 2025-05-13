using System.Drawing.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Reference to the player's transform
    [SerializeField]private Transform playerTransform;
    [SerializeField] private Transform baseTransform;
    [SerializeField] private BeatScoringSystem scoringSystem;

    [SerializeField] private ChildCounter TotalEnemyCounter;
    [SerializeField] private ChildCounter TotalCollectibles;
    // Public getter for the player's transform
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

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Check if an instance already exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy this if another instance exists
        }
        else
        {
            Instance = this; // Set the instance
            DontDestroyOnLoad(gameObject); // Make sure the GameManager persists across scenes
        }
    }

    // Method to set the player's transform
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
        int TotalEnemies = TotalEnemyCounter.CountNestedChildren();
        int totalCollectibles = TotalCollectibles.CountDirectChildren();
        return TotalEnemies + totalCollectibles;
    }
    public int GetTotalSongScore()
    {
        int TotalEnemies = TotalEnemyCounter.CountNestedChildren();

        int maxKillpoints = scoringSystem.MaxWindow;

        int total = (TotalEnemies * maxKillpoints);
        return total;
    }
    public void SaveSongScore()
    {
        
        if (MusicManager.Instance != null)
        {
            /*
            SongData songDataSave = MusicManager.Instance.song;

            songDataSave.FullCombo = getTotalSongs();
            songDataSave.maxPoints = GetTotalSongScore();
            */
            string SongName = MusicManager.Instance.song.songName;
            PlayerPrefs.SetString("SongName", SongName);
        }
        PlayerPrefs.SetInt("TotalMaxPoints",GetTotalSongScore());
        PlayerPrefs.SetInt("FullCombo", GetTotalCombo());
        scoringSystem.SaveScore();
    }
}
