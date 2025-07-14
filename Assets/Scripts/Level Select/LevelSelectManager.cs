using BNG;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public AudioSource audioSource;
    public Text songNameText;
    public Text ArtistText;
    public Text songDescriptionText;
    public ScreenFader screenFader;

    // NEW: Score-related UI fields
    public Text playerScoreText;
    public Text gradeText;
    public Text highestComboText;
    public Text fullComboText;
    public Text maxPointsText;
    [SerializeField] private RotatingLevelSelector selector;

    private SongData selectedSong;

    private void OnEnable()
    {
     
        /*
        if (albumCover.sprite == null)
        {
            setAlbumAlpha(0f);
        }
        songNameText.gameObject.SetActive(false);
        */
    }

    public void LoadSelectedSong()
    {
        if (selectedSong != selector.selectedSong && selector.selectedSong != null)
        {
            SelectSong(selector.selectedSong);
        }
    }

    public void SelectSong(SongData song)
    {
        StopAllCoroutines(); // Stop previous loading if still ongoing
        StartCoroutine(AsyncSelectSong(song));
    }

    private IEnumerator AsyncSelectSong(SongData song)
    {
        selectedSong = song;

        // Text and metadata updates
        songNameText.gameObject.SetActive(true);
        songNameText.text = song.songName;
        ArtistText.text = song.artistName;
        songDescriptionText.text = song.songDescription;

        // Frame delay: allow UI to update
        yield return null;

        // Stop and unload previous clip if necessary
        if (audioSource.isPlaying)
            audioSource.Stop();

        // Wait a short moment before heavy loading
        yield return new WaitForSeconds(0.05f);

        // Load AudioClip and play (simulate async delay if needed)
        if (song.songClip != null)
        {
            audioSource.clip = song.songClip;
            audioSource.PlayDelayed(0.1f); // Delay play slightly to avoid concurrent loading + play spike
        }

        // Delay before UI update
        yield return null;

        // Score display
        if (song.playerScore > 0)
        {
            playerScoreText.gameObject.SetActive(true);
            gradeText.gameObject.SetActive(true);
            playerScoreText.text = song.playerScore.ToString("D7");
            gradeText.text = $"{song.letterGrade}";
        }
        else
        {
            playerScoreText.gameObject.SetActive(false);
            gradeText.gameObject.SetActive(false);
        }
    }


    public void StartSelectedLevel()
    {
        if (selectedSong != null)
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        if (screenFader != null)
        {
            screenFader.DoFadeIn();
            yield return new WaitForSeconds(1f / screenFader.FadeInSpeed);
        }

        SceneLoader loader = GetComponent<SceneLoader>();
        if (loader != null)
        {
            loader.LoadSceneWithCover(selectedSong.sceneName, selectedSong.AlbumCover);
        }
        else
        {
            SceneManager.LoadScene(selectedSong.sceneName); 
        }
    }

    public void GoBack()
    {
        StartCoroutine(FadeAndLoadPreviousScene());
    }

    private IEnumerator FadeAndLoadPreviousScene()
    {
        if (screenFader != null)
        {
            screenFader.DoFadeIn();
            yield return new WaitForSeconds(1f / screenFader.FadeInSpeed);
        }

        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }

    public void ToggleEasyMode(bool toggle)
    {
        PlayerPrefs.SetInt("EasyMode", toggle ? 1 : 0);
    }
}
