using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;  // Needed for scene events

[System.Serializable]
public class SongSaveData
{
    public string songName;
    public int playerScore;
    public string letterGrade;
    public int highestCombo;
    public int fullCombo;
    public int maxPoints;
}

[System.Serializable]
public class SongSaveCollection
{
    public List<SongSaveData> songs = new List<SongSaveData>();
}

public class SongDataSaver : MonoBehaviour
{
    [SerializeField] private List<SongData> songList;
    private string savePath => Path.Combine(Application.persistentDataPath, "songScores.json");

    private void Start()
    {
        LoadFromJson();
        // Register to scene unload event to save automatically on scene change
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        // Unregister event when object destroyed
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene current)
    {
        SaveToJson();
    }

    // Call this from your Score Screen "Close" button or method when exiting score screen UI
    public void OnScoreScreenExit()
    {
        SaveToJson();
    }

    public void SaveToJson()
    {
        var collection = new SongSaveCollection();

        foreach (var song in songList)
        {
            collection.songs.Add(new SongSaveData
            {
                songName = song.songName,
                playerScore = song.playerScore,
                letterGrade = song.letterGrade,
                highestCombo = song.HighestCombo,
                fullCombo = song.FullCombo,
                maxPoints = song.maxPoints
            });
        }

        string json = JsonUtility.ToJson(collection, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Song scores saved to: " + savePath);
    }

    public void LoadFromJson()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found at: " + savePath);
            return;
        }

        string json = File.ReadAllText(savePath);
        SongSaveCollection collection = JsonUtility.FromJson<SongSaveCollection>(json);

        foreach (var save in collection.songs)
        {
            var song = songList.Find(s => s.songName == save.songName);
            if (song != null)
            {
                song.playerScore = save.playerScore;
                song.letterGrade = save.letterGrade;
                song.HighestCombo = save.highestCombo;
                song.FullCombo = save.fullCombo;
                song.maxPoints = save.maxPoints;
            }
        }

        Debug.Log("Song scores loaded from: " + savePath);
    }
}
