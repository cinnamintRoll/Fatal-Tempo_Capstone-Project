using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private string savePath => Path.Combine(Application.persistentDataPath, "songScores.json");

    private void Start()
    {
        LoadFromJson();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene current)
    {
        SaveToJson();
    }

    public void OnScoreScreenExit()
    {
        SaveToJson();
    }

    private List<SongData> GetAllSongsFromAlbums()
    {
        List<SongData> songs = new List<SongData>();

        if (AlbumDatabase.Instance == null)
        {
            Debug.LogWarning("AlbumDatabase instance not found!");
            return songs;
        }

        foreach (var album in AlbumDatabase.Instance.allAlbums)
        {
            songs.AddRange(album.songs);
        }

        return songs;
    }

    public void SaveToJson()
    {
        var collection = new SongSaveCollection();
        var allSongs = GetAllSongsFromAlbums();

        foreach (var song in allSongs)
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
        var allSongs = GetAllSongsFromAlbums();

        foreach (var save in collection.songs)
        {
            var song = allSongs.Find(s => s.songName == save.songName);
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

    public void DeleteAllSaveData()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("All song save data deleted at: " + savePath);
        }
        else
        {
            Debug.Log("No save file found to delete.");
        }

        // Optionally reset in-memory song data too
        var allSongs = GetAllSongsFromAlbums();
        foreach (var song in allSongs)
        {
            song.playerScore = 0;
            song.letterGrade = "";
            song.HighestCombo = 0;
            song.FullCombo = 0;
            song.maxPoints = 0;
        }
    }
}
