using BNG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Intervals
{
    [SerializeField] private float _steps = 1f;
    [SerializeField] private UnityEvent _trigger;

    public float GetIntervalLength(float bpm)
    {
        return 60f / (bpm * _steps);
    }

    public void CheckAndTrigger(double songTime, float bpm, ref double lastTriggerTime)
    {
        double intervalLength = GetIntervalLength(bpm);
        while (songTime >= lastTriggerTime + intervalLength)
        {
            lastTriggerTime += intervalLength;
            _trigger.Invoke();
        }
    }
}

[System.Serializable]
public class BPMSection
{
    public int si;
    public int ei;
    public float sb;
    public float eb;

    public float GetBPM(float sampleRate)
    {
        float duration = (ei - si) / sampleRate;
        float beats = eb - sb;
        return (beats / duration) * 60f;
    }

    public bool ContainsSample(int sampleIndex)
    {
        return sampleIndex >= si && sampleIndex < ei;
    }
}

[System.Serializable]
public class TemporaBPMData
{
    public string version;
    public string songCheckSum;
    public int songSampleCount;
    public int songFrequency;
    public List<BPMSection> bpmData;
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private Intervals[] _objectIntervals;
    [SerializeField] private Intervals _songInterval;

    [Header("Song Interval Settings")]
    public float songIntervalMultiplier = 1f;

    public SongData song;
    public AudioSource musicSource;
    public AudioClip musicClip;
    public float bpm = 120f;
    public float vibrationDuration = 0.1f;
    public float vibrationStrength = 1f;
    public float startDelay = 0f;
    public float musicOffset = 0f;
    public float secondsPerBeat;
    public float startAtTime = 0f;
    public UnityEvent OnIntervalPassed;

    [Header("Optional BPM Map (.dat from Tempora)")]
    public TextAsset bpmDatFile;

    private InputBridge inputBridge;
    private bool isPlaying = false;
    private double dspStartTime;
    private double lastSongIntervalTime;
    private double[] _intervalLastTimes;

    private TemporaBPMData bpmData;
    private float currentBPM;
    private int sampleRate;

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

        if (bpmDatFile != null)
        {
            bpmData = JsonUtility.FromJson<TemporaBPMData>(bpmDatFile.text);
            sampleRate = bpmData.songFrequency;
        }
        else
        {
            sampleRate = AudioSettings.outputSampleRate;
        }

        currentBPM = bpm;
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
        if (!isPlaying)
            return;

        // Adjust pitch to match time scale
        musicSource.pitch = Mathf.Max(Time.timeScale, 0f);

        double songTime = (AudioSettings.dspTime - dspStartTime) * Time.timeScale;

        if (musicSource.clip == null)
            return;

        int currentSample = musicSource.timeSamples;

        // Update BPM from .dat if available
        if (bpmData != null && bpmData.bpmData != null)
        {
            foreach (var section in bpmData.bpmData)
            {
                if (section.ContainsSample(currentSample))
                {
                    currentBPM = section.GetBPM(sampleRate);
                    break;
                }
            }
        }
        else
        {
            currentBPM = bpm;
        }

        double interval = 60.0 / (currentBPM * songIntervalMultiplier);
        while (songTime >= lastSongIntervalTime + interval)
        {
            lastSongIntervalTime += interval;
            OnIntervalPassed.Invoke();
            StartCoroutine(VibrateController(ControllerHand.Left));
            StartCoroutine(VibrateController(ControllerHand.Right));
        }

        for (int i = 0; i < _objectIntervals.Length; i++)
        {
            _objectIntervals[i].CheckAndTrigger(songTime, currentBPM, ref _intervalLastTimes[i]);
        }
    }


    public void PlayMusic()
    {
        if (musicClip != null && !isPlaying)
        {
            if (_objectIntervals != null)
            {
                _intervalLastTimes = new double[_objectIntervals.Length];
                for (int i = 0; i < _intervalLastTimes.Length; i++)
                    _intervalLastTimes[i] = 0f;
            }

            lastSongIntervalTime = 0f;

            double delay = Mathf.Max(0f, startDelay + musicOffset);
            dspStartTime = AudioSettings.dspTime + delay;

            if (startAtTime > 0f)
            {
                musicSource.time = startAtTime;
            }

            musicSource.PlayScheduled(dspStartTime);
            isPlaying = true;

            OnIntervalPassed.Invoke();
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

    public float GetCurrentBPM()
    {
        return currentBPM;
    }
}
