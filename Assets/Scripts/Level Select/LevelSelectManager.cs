using BNG;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public AudioSource audioSource;
    public Text songNameText;
    public Text songDescriptionText;
    public ScreenFader screenFader;
    public Image albumCover;

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

    public void setAlbumAlpha(float alpha)
    {
        var tempcolor = albumCover.color;
        tempcolor.a = alpha;
        albumCover.color = tempcolor;
    }

    public void LoadSelectedSong()
    {
        if (selectedSong != selector.selectedSong)
        {
            SelectSong(selector.selectedSong);
        }
    }

    public void SelectSong(SongData song)
    {
        selectedSong = song;

        songNameText.gameObject.SetActive(true);
        songNameText.text = song.songName;
        songDescriptionText.text = song.songDescription;
        //albumCover.sprite = song.AlbumCover;
        //setAlbumAlpha(1f);

        // Play the song preview
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.clip = song.songClip;
        audioSource.Play();

        // Show or hide score and grade based on stored score
        bool hasScore = song.playerScore > 0;

        playerScoreText.gameObject.SetActive(hasScore);
        gradeText.gameObject.SetActive(hasScore);

        if (hasScore)
        {
            playerScoreText.text = $"{song.playerScore}";
            gradeText.text = $"{song.letterGrade}";
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

        SceneManager.LoadScene(selectedSong.sceneName);
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
}
