using BNG;
using System.Collections;
using UnityEngine; // Ensure you have the VR Interaction Framework namespace

public class BeatTiming : MonoBehaviour
{
    public static BeatTiming Instance { get; private set; } // Singleton instance

    public float bpm = 120f; // Beats per minute
    public float vibrationDuration = 0.1f; // Duration of the vibration
    public float vibrationStrength = 1f; // Vibration strength (0 to 1)

    private float secondsPerBeat; // Time in seconds for each beat
    private InputBridge inputBridge;

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
        // Calculate the seconds per beat based on BPM
        secondsPerBeat = 60f / bpm;
        StartCoroutine(VibrateOnBeat());
    }

    private IEnumerator VibrateOnBeat()
    {
        while (true)
        {
            // Wait for the duration of one beat
            yield return new WaitForSeconds(secondsPerBeat);

            // Vibrate both controllers
            VibrateControllers();
        }
    }

    private void VibrateControllers()
    {
        // Clamp the vibration strength to be between 0 and 1
        float clampedVibrationStrength = Mathf.Clamp(vibrationStrength, 0f, 1f);
        // Clamp the vibration duration to be non-negative
        float clampedVibrationDuration = Mathf.Max(vibrationDuration, 0f);

        Debug.Log($"Vibration Strength: {clampedVibrationStrength}, Duration: {clampedVibrationDuration}");

        // Vibrate the left controller
        inputBridge.VibrateController(clampedVibrationStrength, clampedVibrationStrength, clampedVibrationDuration, ControllerHand.Left);
        // Vibrate the right controller
        inputBridge.VibrateController(clampedVibrationStrength, clampedVibrationStrength, clampedVibrationDuration, ControllerHand.Right);
    }


    // Optional: Method to change BPM during runtime
    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        secondsPerBeat = 60f / bpm; // Recalculate the seconds per beat
    }
}
