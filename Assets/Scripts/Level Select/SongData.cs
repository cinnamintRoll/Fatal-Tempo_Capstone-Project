using UnityEngine;

[CreateAssetMenu(fileName = "New Song Data", menuName = "Level Select/Song Data")]
public class SongData : ScriptableObject
{
    public string songName;

    [TextArea(3, 10)]  // Multiline text area with min 3 and max 10 lines
    public string songDescription;

    public Sprite AlbumCover;
    public AudioClip songClip;
    public string sceneName;  // Name of the scene to load

    // Performance-related fields
    public int playerScore;
    public string letterGrade; // Example: "S", "A", "B", etc.
    public int HighestCombo;
    public int FullCombo;      // Number of successful hits
    public int maxPoints;        // Total number of possible hits
}
