using System.Collections.Generic;
using UnityEngine;

public class SongProgressionManager : MonoBehaviour
{
    [Tooltip("Assign the level selector to update its album songs.")]
    public RotatingLevelSelector levelSelector;

    private readonly List<string> gradeOrder = new List<string> { "F", "C", "B", "A", "S", "SS" };
    private const string requiredGrade = "B";
    private bool demoMode = false;
    private void Start()
    {
        demoMode = PlayerPrefs.GetInt("DemoMode", 0) == 1;
        ApplyProgression();
    }

    public void ApplyProgression()
    {
        if (levelSelector == null || levelSelector.CurrentAlbum == null)
        {
            Debug.LogWarning("LevelSelector or CurrentAlbum is null.");
            return;
        }

        List<SongData> songs = levelSelector.CurrentAlbum.songs;

        for (int i = 0; i < songs.Count; i++)
        {
            if (demoMode)
            {
                songs[i].locked = false;
                continue;
            }

            if (i == 0)
            {
                // First song is always unlocked
                songs[i].locked = false;
                continue;
            }

            SongData previousSong = songs[i - 1];

            bool unlocked = IsGradeSufficient(previousSong.letterGrade);
            songs[i].locked = !unlocked;
        }

        // Refresh UI visuals
        levelSelector.SelectAlbum(levelSelector.CurrentAlbum);
    }

    private bool IsGradeSufficient(string grade)
    {
        if (string.IsNullOrEmpty(grade)) return false;

        int actualIndex = gradeOrder.IndexOf(grade.ToUpper());
        int requiredIndex = gradeOrder.IndexOf(requiredGrade);

        return actualIndex >= requiredIndex;
    }
}
