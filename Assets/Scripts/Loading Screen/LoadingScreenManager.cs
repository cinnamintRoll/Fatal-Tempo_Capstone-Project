using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Image albumCoverImage;               // Assign in Inspector

    public float delayBeforeLoad = 0.5f;        // Time before loading starts
    public float delayBeforeFadeOut = 0.5f;     // Time after scene loads, before fade-out
    public float delayAfterFadeOut = 0.5f;      // Time after fade-out, before scene activation

    private string sceneToLoad;
    private BNG.ScreenFader screenFader;

    private void Start()
    {
        sceneToLoad = PlayerPrefs.GetString("SceneToLoad");

        // Set album cover if available
        if (albumCoverImage != null && LoadDataCarrier.AlbumCover != null)
        {
            albumCoverImage.sprite = LoadDataCarrier.AlbumCover;
        }
        else
        {
            albumCoverImage.gameObject.SetActive(false);
        }

        screenFader = FindObjectOfType<BNG.ScreenFader>();

        StartCoroutine(LoadSequence());
    }

    IEnumerator LoadSequence()
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        yield return StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(delayBeforeFadeOut);

        if (screenFader != null)
        {
            screenFader.DoFadeIn(); // Fade out to transparent
        }

        float fadeOutDuration = screenFader != null ? 1f / screenFader.FadeOutSpeed : 0.5f;
        yield return new WaitForSeconds(fadeOutDuration + delayAfterFadeOut);

        asyncLoad.allowSceneActivation = true;
    }
}
