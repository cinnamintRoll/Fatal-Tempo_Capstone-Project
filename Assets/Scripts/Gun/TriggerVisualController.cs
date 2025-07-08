using UnityEngine;
using BNG;

public class TriggerVisualController : MonoBehaviour
{
    public Transform triggerTransform;  // The transform of the gun's trigger
    public Vector3 startRotation;       // The original rotation of the trigger
    public Vector3 pressedRotation;     // The rotation when the trigger is fully pressed
    public float smoothTime = 0.1f;     // Smoothing factor for the trigger animation

    private InputBridge input;
    private float currentTriggerValue;
    private Vector3 currentRotation;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        input = InputBridge.Instance;
        currentRotation = triggerTransform.localEulerAngles;
    }

    void Update()
    {
        // Get trigger value from the InputBridge (right trigger)
        currentTriggerValue = input.RightTrigger;

        // Smoothly interpolate the trigger's rotation between the start and pressed rotations
        Vector3 targetRotation = Vector3.Lerp(startRotation, pressedRotation, currentTriggerValue);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref velocity, smoothTime);

        // Apply the rotation to the trigger transform
        triggerTransform.localEulerAngles = currentRotation;
    }
}
