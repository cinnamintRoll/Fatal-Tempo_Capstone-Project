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
}
