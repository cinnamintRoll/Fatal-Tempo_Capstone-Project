using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public float fadeDelay = 0.5f; // Optional wait after fade before loading
    private BNG.ScreenFader screenFader;

    public void LoadSceneWithCover(string sceneName, Sprite albumCover = null)
    {
        PlayerPrefs.SetString("SceneToLoad", sceneName);

        if (albumCover != null)
        {
            LoadDataCarrier.AlbumCover = albumCover;
        }
        else
        {
            LoadDataCarrier.AlbumCover = null;
        }

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        screenFader = FindObjectOfType<BNG.ScreenFader>();

        if (screenFader != null)
        {
            screenFader.DoFadeIn(); 
            float fadeTime = 1f / Mathf.Max(screenFader.FadeInSpeed, 0.01f);
            yield return new WaitForSeconds(fadeTime + fadeDelay);
        }

        SceneManager.LoadScene("LoadingScreen");
    }
}
