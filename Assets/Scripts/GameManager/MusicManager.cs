using BNG;
using JetBrains.Annotations;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Intervals 
{
    [SerializeField] private float _steps;
    [SerializeField] private UnityEvent _trigger;
    private int _lastInterval;
     public float GetIntervalLength(float bpm)
    {
        return 16f /(bpm * _steps);
    }

    public bool CheckForNewInterval(float interval)
    {
        if(Mathf.FloorToInt(interval) != _lastInterval)
        {
            _lastInterval = Mathf.FloorToInt(interval); 
            _trigger.Invoke();  
            return true;
        }
        return false;
    }
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; } // Singleton instance
    [SerializeField] private Intervals[] _objectIntervals;
    [SerializeField] private Intervals _songInterval;
    public AudioSource musicSource; // AudioSource for the music
    public AudioClip musicClip; // The music clip to be played
    public float bpm = 120f; // Beats per minute
    public float vibrationDuration = 0.1f; // Duration of the vibration
    public float vibrationStrength = 1f; // Vibration strength (0 to 1)
    public float startDelay = 0f; // Delay before everything starts
    public float musicOffset = 0f; // Offset for when the music starts (can be negative or positive)
    public float secondsPerBeat; // Time in seconds for each beat
    private InputBridge inputBridge;
    private bool isPlaying = false; // Track if music is playing
    private float startTime; // To track when the music starts in game time
    public UnityEvent OnIntervalPassed;
    public SongData song;
    private void Awake()
    {
        // Implement the Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate MusicManager instance detected and will be destroyed.");
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject); // Optional: keep this instance across scenes

        inputBridge = InputBridge.Instance;
        OnIntervalPassed = new UnityEvent();
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

    private void Update()
    {
        // Sync music speed with game time scale
        musicSource.pitch = Time.timeScale;

        // Calculate the current time in seconds from the start of the music
        float elapsedTime = musicSource.time + musicOffset; // Add music offset to sync if needed

        // Calculate how many beats have passed
        float beatsPassed = elapsedTime / secondsPerBeat;

        // If a new beat has passed (by comparing to the previous beat)
        if (Mathf.FloorToInt(beatsPassed) > Mathf.FloorToInt((elapsedTime - Time.deltaTime) / secondsPerBeat))
        {
            // Trigger vibrations (you can trigger them here or in the interval system)
            StartCoroutine(VibrateController(ControllerHand.Left));
            StartCoroutine(VibrateController(ControllerHand.Right));
        }

        // Sync intervals based on beats
        float sampledTime = (musicSource.timeSamples / (musicSource.clip.frequency * _songInterval.GetIntervalLength(bpm)));
        if (_songInterval.CheckForNewInterval(sampledTime))
        {
            OnIntervalPassed.Invoke();
            // Trigger vibrations or actions based on the interval
        }

        foreach (Intervals interval in _objectIntervals)
        {
            float intervalSampledTime = (musicSource.timeSamples / (musicSource.clip.frequency * interval.GetIntervalLength(bpm)));
            interval.CheckForNewInterval(intervalSampledTime);
        }
    }


    public void PlayMusic()
    {
        if (musicClip != null && !isPlaying)
        {
            StartCoroutine(StartEverythingWithDelay());
            isPlaying = true;
        }
    }

    private IEnumerator StartEverythingWithDelay()
    {
        // Wait for the start delay before anything starts
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        // Record the actual start time
        startTime = Time.time;
        musicSource.Play(); // Play music at the correct offset
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

    // Sync the vibrations to the game time instead of relying on WaitForSeconds
    private void SyncBeatsToGameTime()
    {
        float elapsedTime = Time.time - startTime; // Time since music started
        float musicTime = musicSource.time; // Current music playback time

        // Calculate how many beats have passed since start
        float beatsPassed = elapsedTime / secondsPerBeat;

        // If we're at or past a beat, trigger the vibration
        if (Mathf.FloorToInt(beatsPassed) > Mathf.FloorToInt((elapsedTime - Time.deltaTime) / secondsPerBeat))
        {
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
