using BNG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Intervals
{
    [SerializeField] private float _steps;
    [SerializeField] private UnityEvent _trigger;
    private float _lastTriggerTime;

    public float GetIntervalLength(float bpm)
    {
        return 16f / (bpm * _steps);
    }

    public void CheckAndTrigger(float currentTime, float bpm)
    {
        float intervalLength = GetIntervalLength(bpm);
        while (currentTime >= _lastTriggerTime + intervalLength)
        {
            _lastTriggerTime += intervalLength;
            _trigger.Invoke();
        }
    }

    public void Reset()
    {
        _lastTriggerTime = 0f;
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
    [Tooltip("If greater than 0, skips startDelay and starts at this time in seconds.")]
    public float startAtTime = 0f;

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

        float currentTime = musicSource.time;

        float songIntervalLength = _songInterval.GetIntervalLength(bpm);
        while (currentTime >= _lastSongIntervalTime + songIntervalLength)
        {
            _lastSongIntervalTime += songIntervalLength;
            OnIntervalPassed.Invoke();
            StartCoroutine(VibrateController(ControllerHand.Left));
            StartCoroutine(VibrateController(ControllerHand.Right));
        }

        foreach (Intervals interval in _objectIntervals)
        {
            interval.CheckAndTrigger(currentTime, bpm);
        }
    }

    public void PlayMusic()
    {
        if (musicClip != null && !isPlaying)
        {
            foreach (var interval in _objectIntervals)
                interval.Reset();

            _songInterval.Reset();

            if (startAtTime > 0f)
            {
                musicSource.time = Mathf.Clamp(startAtTime, 0f, musicClip.length);

                _lastSongIntervalTime = 0f;
                musicSource.Play();
                startTime = Time.time - startAtTime;
                isPlaying = true;
            }
            else
            {
                StartCoroutine(StartEverythingWithDelay());
                isPlaying = true;
            }
            OnIntervalPassed.Invoke();
        }
    }

    private IEnumerator StartEverythingWithDelay()
    {
        float totalDelay = Mathf.Max(0f, startDelay + musicOffset); // Apply offset as additional delay
        if (totalDelay > 0f)
        {
            yield return new WaitForSeconds(totalDelay);
        }

        startTime = Time.time;
        musicSource.time = 0f; // Start from beginning of the song
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

        inputBridge.VibrateController(clampedVibrationStrength, clampedVibrationStrength, clampedVibrationDuration, hand);
        yield return null;
    }

    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        secondsPerBeat = 60f / bpm;
    }
}
