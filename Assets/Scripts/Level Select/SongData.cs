using UnityEngine;

[CreateAssetMenu(fileName = "New Song Data", menuName = "Level Select/Song Data")]
public class SongData : ScriptableObject
{
    public string songName;
    public AudioClip songClip;
    public string sceneName;  // Name of the scene to load
}
