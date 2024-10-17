using BNG;
using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; } // Singleton instance

    public AudioSource musicSource; // AudioSource for the music
    public AudioClip musicClip; // The music clip to be played
    public float bpm = 120f; // Beats per minute
    public float vibrationDuration = 0.1f; // Duration of the vibration
    public float vibrationStrength = 1f; // Vibration strength (0 to 1)

    private float secondsPerBeat; // Time in seconds for each beat
    private InputBridge inputBridge;
    private bool isPlaying = false; // Track if music is playing

    private void Awake()
    {
        // Implement the Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keep this instance across scenes

        inputBridge = InputBridge.Instance;
    }

    private void Start()
    {
        // Initialize AudioSource and music clip
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.clip = musicClip;

        // Calculate the seconds per beat based on BPM
        secondsPerBeat = 60f / bpm;
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (musicClip != null && !isPlaying)
        {
            musicSource.Play();
            StartCoroutine(VibrateOnBeat());
            isPlaying = true;
        }
    }

    public void StopMusic()
    {
        if (isPlaying)
        {
            musicSource.Stop();
            StopAllCoroutines();
            isPlaying = false;
        }
    }

    private IEnumerator VibrateOnBeat()
    {
        while (musicSource.isPlaying)
        {
            // Wait for the duration of one beat
            yield return new WaitForSeconds(secondsPerBeat);

            // Start vibration for both controllers in parallel
            StartCoroutine(VibrateController(ControllerHand.Left));
            StartCoroutine(VibrateController(ControllerHand.Right));
        }
    }

    private IEnumerator VibrateController(ControllerHand hand)
    {
        // Clamp the vibration strength and duration
        float clampedVibrationStrength = Mathf.Clamp(vibrationStrength, 0f, 1f);
        float clampedVibrationDuration = Mathf.Max(vibrationDuration, 0f);

        Debug.Log($"Vibration Strength: {clampedVibrationStrength}, Duration: {clampedVibrationDuration}, Hand: {hand}");

        // Vibrate the controller for the given hand
        inputBridge.VibrateController(clampedVibrationStrength, clampedVibrationStrength, clampedVibrationDuration, hand);

        yield return null; // Optional: adjust this to control vibration timing
    }


    // Optional: Method to change BPM during runtime
    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        secondsPerBeat = 60f / bpm; // Recalculate the seconds per beat
    }
}
