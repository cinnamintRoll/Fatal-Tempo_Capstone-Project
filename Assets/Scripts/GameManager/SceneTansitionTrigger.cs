using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTansitionTrigger : MonoBehaviour
{
    [SerializeField] private ScreenFader screenFader; // Reference to the ScreenFader
    [SerializeField] private LayerMask layerMask;     // LayerMask to filter objects (e.g., Player layer)
    [SerializeField] private string SceneName = "Main Menu"; // The name of the main menu scene
    [SerializeField] private float fadeInDelay = 1.0f; // Optional delay before loading the scene

    // Update is called once per frame
    void Update()
    {

    }

    void MoveBacktoHomeScreen()
    {
        // Start the fade in (to black) and wait until it's done before loading the main menu scene
        StartCoroutine(FadeAndLoadScene());
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that triggered the collider is in the specified layer (e.g., Player layer)
        if (((1 << other.gameObject.layer) & layerMask.value) != 0)
        {
            // Trigger the fade in (to black) and scene change
            MoveBacktoHomeScreen();
        }
    }

    // Coroutine to handle fading in (to black) and loading the main menu scene
    private IEnumerator FadeAndLoadScene()
    {
        // Start the fade-in animation (fade to black)
        if (screenFader != null)
        {
            screenFader.DoFadeIn(); // Fade to black
        }

        // Wait for the fade-in duration (plus any additional delay if desired)
        yield return new WaitForSeconds(fadeInDelay);

        // Load the main menu scene
        SceneManager.LoadScene(SceneName);
    }
}
