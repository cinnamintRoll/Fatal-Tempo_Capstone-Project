using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Album", menuName = "Level Select/Album Data")]
public class AlbumData : ScriptableObject
{
    public string albumName;

    [TextArea(3, 10)]
    public string albumDescription;

    public Sprite albumCover;

    public List<SongData> songs;
}
