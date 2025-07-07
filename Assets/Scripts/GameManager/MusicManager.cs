using BNG;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Intervals
{
    [SerializeField] private float _steps = 1f; // 1 = once per beat, 2 = twice per beat
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

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private Intervals[] _objectIntervals;
    [SerializeField] private Intervals _songInterval;

    [Header("Song Interval Settings")]
    public float songIntervalMultiplier = 1f; // 1 = every beat, 2 = twice per beat, 0.5 = every 2 beats, etc.

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

    public UnityEvent OnIntervalPassed;
    public float startAtTime = 0f;

    private double dspStartTime;
    private double lastSongIntervalTime;

    private double[] _intervalLastTimes;

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
        if (!isPlaying)
            return;

        double songTime = AudioSettings.dspTime - dspStartTime;

        double interval = 60.0 / (bpm * songIntervalMultiplier);
        while (songTime >= lastSongIntervalTime + interval)
        {
            lastSongIntervalTime += interval;
            OnIntervalPassed.Invoke();
            StartCoroutine(VibrateController(ControllerHand.Left));
            StartCoroutine(VibrateController(ControllerHand.Right));
        }

        for (int i = 0; i < _objectIntervals.Length; i++)
        {
            _objectIntervals[i].CheckAndTrigger(songTime, bpm, ref _intervalLastTimes[i]);
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

            // Set playback time BEFORE scheduling
            if (startAtTime > 0f)
            {
                musicSource.time = startAtTime;
            }

            musicSource.PlayScheduled(dspStartTime);
            isPlaying = true;

            // Immediate visual feedback if needed
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
}
