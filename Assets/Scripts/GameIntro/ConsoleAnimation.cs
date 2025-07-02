using BNG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // For scene management
using System.Collections;

public class StartAnimIntro : MonoBehaviour
{
    // The Animator component attached to the GameObject
    private Animator animator;

    // UnityEvent that gets triggered when the animation starts
    [SerializeField] private UnityEvent OnAnimationStart;

    // Name of the animation trigger
    [SerializeField] private string animationTriggerName = "StartAnim";

    // Reference to the ScreenFader instance in the scene
    private ScreenFader screenFader;

    // The Renderer component to change material color
    [SerializeField] private Renderer objectRenderer; // Reference to the Renderer

    // Fade duration in seconds
    private float fadeDuration = 0.5f;

    private void Awake()
    {
        // Get the Animator component from the GameObject
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component not found on the GameObject.");
        }

        // Try to find the ScreenFader instance in the scene
        screenFader = FindObjectOfType<ScreenFader>();

        if (screenFader == null)
        {
            Debug.LogError("No ScreenFader instance found in the scene.");
        }

        // Get the Renderer component
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            Debug.LogError("Renderer component not found on the GameObject.");
        }
    }

    // Function to manually start the animation and trigger the Unity event
    public void StartAnimation()
    {
        if (animator != null)
        {
            // Set the trigger to start the animation
            animator.SetTrigger(animationTriggerName);

            // Invoke the Unity event to notify listeners
            OnAnimationStart?.Invoke();
        }
    }

    // Coroutine to wait for the animation to finish and then change the scene
    private void ChangeScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Function to fade the screen to white
    public void FadeToWhite()
    {
        if (screenFader != null)
        {
            // Set the fade color to white
            screenFader.FadeColor = Color.white;
            screenFader.DoFadeIn(); // Start fade to white
        }
        else
        {
            Debug.LogError("ScreenFader instance not found.");
        }
    }

    // Function to fade the material from black to white over fadeDuration
    public void FadeMaterialBlackToWhite()
    {
        if (objectRenderer != null)
        {
            // Start the coroutine to fade the material
            StartCoroutine(FadeMaterialCoroutine(Color.black, Color.white, fadeDuration));
        }
        else
        {
            Debug.LogError("Renderer instance not found.");
        }
    }

    // Coroutine to fade the material color from one color to another over time
    private IEnumerator FadeMaterialCoroutine(Color startColor, Color endColor, float duration)
    {
        float elapsedTime = 0f;

        // Get the initial material color
        Material material = objectRenderer.material; // This creates a new instance of the material
        material.color = startColor; // Set initial color

        // Gradually change the color over time
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            material.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }

        // Ensure the final color is set
        material.color = endColor;
    }
}
