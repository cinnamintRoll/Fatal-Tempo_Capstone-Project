using System;
using System.Collections.Generic;
using UnityEngine;

public class AlbumDatabase : MonoBehaviour
{
    public static AlbumDatabase Instance { get; private set; }

    [Header("All Albums In This Scene")]
    public List<AlbumData> allAlbums = new List<AlbumData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject); // Ensures only one instance
            return;
        }

        Instance = this;
    }

    public SongData GetSongFromAlbums(string SongName)
    {
    if (string.IsNullOrEmpty(SongName)) return null;

        foreach (AlbumData albumData in allAlbums) 
        {
            foreach(SongData Song in albumData.songs)
            {
                if (string.Equals(Song.songName, SongName, StringComparison.OrdinalIgnoreCase)) return Song;
            }
        }
        return null;
    }
}
