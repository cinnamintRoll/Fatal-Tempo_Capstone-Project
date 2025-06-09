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
        return 16f / (bpm * _steps);
    }

    public bool CheckForNewInterval(float interval)
    {
        if (Mathf.FloorToInt(interval) != _lastInterval)
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
    public static MusicManager Instance { get; private set; }
    [SerializeField] private Intervals[] _objectIntervals;
    [SerializeField] private Intervals _songInterval;
    public SongData song;
    public AudioSource musicSource;
    public AudioClip musicClip;
    public float bpm = 120f;
    public float vibrationDuration = 0.1f;
    public float vibrationStrength = 1f;
    public float startDelay = 0f;
    public float musicOffset = 0f;
    public float secondsPerBeat;
    private InputBridge inputBridge;
    private bool isPlaying = false;
    private float startTime;
    public UnityEvent OnIntervalPassed;
    

    private float _lastSongIntervalTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate MusicManager instance detected and will be destroyed.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        inputBridge = InputBridge.Instance;
        OnIntervalPassed = new UnityEvent();
    }

    private void Start()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.clip = musicClip;
        secondsPerBeat = 60f / bpm;
        PlayMusic();
    }

    private void Update()
    {
        musicSource.pitch = Time.timeScale;

        float currentTime = musicSource.time + musicOffset;
        float intervalLength = _songInterval.GetIntervalLength(bpm);

        while (currentTime >= _lastSongIntervalTime + intervalLength)
        {
            _lastSongIntervalTime += intervalLength;
            OnIntervalPassed.Invoke();
            StartCoroutine(VibrateController(ControllerHand.Left));
            StartCoroutine(VibrateController(ControllerHand.Right));
        }

        float beatsPassed = currentTime / secondsPerBeat;
        if (Mathf.FloorToInt(beatsPassed) > Mathf.FloorToInt((currentTime - Time.deltaTime) / secondsPerBeat))
        {
            
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
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }
        startTime = Time.time;
        musicSource.Play();
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

    private IEnumerator VibrateController(ControllerHand hand)
    {
        float clampedVibrationStrength = Mathf.Clamp(vibrationStrength, 0f, 1f);
        float clampedVibrationDuration = Mathf.Max(vibrationDuration, 0f);

        Debug.Log($"Vibration Strength: {clampedVibrationStrength}, Duration: {clampedVibrationDuration}, Hand: {hand}");

        inputBridge.VibrateController(clampedVibrationStrength, clampedVibrationStrength, clampedVibrationDuration, hand);
        yield return null;
    }

    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        secondsPerBeat = 60f / bpm;
    }
}