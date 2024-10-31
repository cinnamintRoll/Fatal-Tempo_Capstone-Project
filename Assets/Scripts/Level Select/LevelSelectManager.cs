using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public AudioSource audioSource;  // Reference to AudioSource to play song previews
    public Text songNameText;        // UI Text to display the selected song name
    private SongData selectedSong;   // Currently selected song data

    // Call this method when a song button is clicked
    public void SelectSong(SongData song)
    {
        selectedSong = song;
        songNameText.text = song.songName;

        // Play the song preview
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.clip = song.songClip;
        audioSource.Play();
    }

    // Call this method when the Start button is pressed
    public void StartSelectedLevel()
    {
        if (selectedSong != null)
        {
            SceneManager.LoadScene(selectedSong.sceneName);
        }
    }
}
