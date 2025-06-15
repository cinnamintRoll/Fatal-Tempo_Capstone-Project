using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Song Data", menuName = "Level Select/Song Data")]
public class SongData : ScriptableObject
{
    public string songName;
    public string artistName;

    [TextArea(3, 10)]
    public string songDescription;

    public Sprite AlbumCover;
    public AudioClip songClip;

#if UNITY_EDITOR
    public SceneAsset sceneAsset; // Drag-and-drop scene reference in Editor
#endif

    // For runtime use
    [HideInInspector]
    public string sceneName;

    public int playerScore;
    public string letterGrade;
    public int HighestCombo;
    public int FullCombo;
    public int maxPoints;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            string path = AssetDatabase.GetAssetPath(sceneAsset);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
#endif
}
