using BNG;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public AudioSource audioSource;     // Reference to AudioSource to play song previews
    public Text songNameText;           // UI Text to display the selected song name
    public Text songDescriptionText;    // UI Text to display the song description
    public ScreenFader screenFader;     // Reference to the ScreenFader script
    public Image albumCover;
    private SongData selectedSong;      // Currently selected song data

    private void OnEnable()
    {
        if (albumCover.sprite == null)
        {
            setAlbumAlpha(0f);
        }
    }

    public void setAlbumAlpha(float alpha)
    {
        var tempcolor = albumCover.color;
        tempcolor.a = alpha;
        albumCover.color = tempcolor;
    }
    // Call this method when a song button is clicked
    public void SelectSong(SongData song)
    {
        selectedSong = song;
        songNameText.text = song.songName;
        songDescriptionText.text = song.songDescription;  // Display the song description
        albumCover.sprite = song.AlbumCover;
        setAlbumAlpha(1f);
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
            StartCoroutine(FadeAndLoadScene());
        }
    }

    // Coroutine to handle fade-out and scene loading
    private IEnumerator FadeAndLoadScene()
    {
        if (screenFader != null)
        {
            screenFader.DoFadeIn();  // Trigger fade-in (screen darkening)
            yield return new WaitForSeconds(1f / screenFader.FadeInSpeed);  // Wait for fade-in to complete
        }

        SceneManager.LoadScene(selectedSong.sceneName);
    }

    // Call this method when the Back button is pressed
    public void GoBack()
    {
        StartCoroutine(FadeAndLoadPreviousScene());
    }

    // Coroutine for fading out before going back to the previous scene
    private IEnumerator FadeAndLoadPreviousScene()
    {
        if (screenFader != null)
        {
            screenFader.DoFadeIn();  // Trigger fade-in (screen darkening)
            yield return new WaitForSeconds(1f / screenFader.FadeInSpeed);  // Wait for fade-in to complete
        }

        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }
}
