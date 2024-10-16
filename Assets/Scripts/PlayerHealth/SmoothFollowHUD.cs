using UnityEngine;

public class SmoothFollowHUD : MonoBehaviour
{
    public Transform playerCamera;   // Reference to the player's camera (VR headset)
    public float followDistance = 2f;  // Distance in front of the player
    public float followHeight = 0.5f;  // Height offset from the player's view
    public float smoothSpeed = 5f;     // Speed of the smooth movement
    public float rotationSpeed = 5f;   // Speed of smooth rotation
    public float tiltOffset = 10f;     // Tilt angle in degrees
    public float minHeight = 0.5f;     // Minimum height above the ground to avoid sinking

    private Vector3 targetPosition;

    private void Start()
    {
        if (!playerCamera)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void FixedUpdate()
    {
        // Target position is in front of the player's camera, at a slight height offset
        targetPosition = playerCamera.position + playerCamera.forward * followDistance;
        targetPosition.y += followHeight;

        // Ensure the targetPosition.y doesn't go below the minimum height
        if (targetPosition.y < minHeight)
        {
            targetPosition.y = minHeight;
        }

        // Smoothly move the UI to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * smoothSpeed);

        // Apply tilt to the HUD by rotating around the right axis (X-axis)
        Quaternion tiltRotation = Quaternion.Euler(tiltOffset, 0, 0);

        // Smoothly rotate the UI to face the same direction as the player's camera, with tilt offset
        Quaternion targetRotation = Quaternion.LookRotation(playerCamera.forward) * tiltRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.unscaledDeltaTime * rotationSpeed);
    }
}
