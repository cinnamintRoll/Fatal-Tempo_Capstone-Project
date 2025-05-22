using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Animator animator;  // Reference to Animator
    [SerializeField] private string FadeInTrigger = "Play"; // Trigger parameter name
    [SerializeField] private string FadeOutTrigger = "Disappear";

    [Header("Animation Settings")]
    [SerializeField] private bool playAnimationOnStart = true;

    [Header("Auto Fade Settings")]
    [SerializeField] private bool fadeOutAfterSeconds = false;
    [SerializeField] private float fadeOutDelay = 2f;

    private void Start()
    {
        if (playAnimationOnStart)
        {
            PlayAnimation(FadeInTrigger);
        }
    }

    public void SetText(string text)
    {
        scoreText.text = text;
    }

    public void SetLocation(Vector3 location)
    {
        transform.position = location;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    public void PlayAnimation(string Trigger)
    {
        if (animator != null && !string.IsNullOrEmpty(Trigger))
        {
            animator.SetTrigger(Trigger);
        }
    }

    public void FadeOut()
    {
        PlayAnimation(FadeOutTrigger);
    }

    public void FadeIn()
    {
        PlayAnimation(FadeInTrigger);
    }

    public void FadeOutAnim()
    {
        if (fadeOutAfterSeconds)
        {
            StartCoroutine(AutoFadeOutRoutine());
        }
    }

    private System.Collections.IEnumerator AutoFadeOutRoutine()
    {
        yield return new WaitForSeconds(fadeOutDelay);
        FadeOut();
    }

    private void Update()
    {
        //Debug.Log(this.transform.position);
    }
}
