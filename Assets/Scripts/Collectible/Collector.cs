using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component can be added to any object that is capable of collecting items.
/// </summary>
public class Collector : MonoBehaviour
{
    public BNG.ControllerHand HandSide = ControllerHand.None;
    public Vector3 CollectorVelocity;
    public int TotalItemsCollected = 0; // Total number of collected items
    private Vector3 PreviousPosition;
    private InputBridge inputBridge;

    public float vibrationDuration = 0.1f; // Duration of the vibration
    public float vibrationStrength = 1f; // Vibration strength (0 to 1)
    public void CollectItem(int itemValue)
    {
        float clampedVibrationStrength = Mathf.Clamp(vibrationStrength, 0f, 1f);
        // Clamp the vibration duration to be non-negative
        float clampedVibrationDuration = Mathf.Max(vibrationDuration, 0f);

        if (HandSide != ControllerHand.None)
        {
            inputBridge.VibrateController(clampedVibrationStrength, clampedVibrationStrength, clampedVibrationDuration, HandSide);
        }
        TotalItemsCollected += itemValue;
        Debug.Log("Collected Item! Total Items: " + TotalItemsCollected);
    }
    private void Start()
    {
        PreviousPosition = transform.position;
        inputBridge = InputBridge.Instance;
    }

    private void Update()
    {
        CollectorVelocity = (transform.position - PreviousPosition) / Time.deltaTime;
        PreviousPosition = transform.position;
    }
}

