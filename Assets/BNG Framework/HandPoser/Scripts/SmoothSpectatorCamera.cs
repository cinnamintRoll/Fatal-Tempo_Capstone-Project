using UnityEngine;

public class SmoothSpectatorCamera : MonoBehaviour
{
    public Transform target; // Assign your VR headset camera here

    [Header("View Mode")]
    public bool useThirdPerson = false;

    [Header("Offsets")]
    public Vector3 firstPersonOffset = new Vector3(0f, 0.05f, -0.1f);
    public Vector3 thirdPersonOffset = new Vector3(0f, 2f, -4f);

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.05f;

    private Vector2 velocityXZ;       // For 3rd-person XZ smoothing
    private Vector3 velocity3D;       // For 1st-person full smoothing
    private Quaternion currentRotation;

    void Start()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        currentRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float deltaTime = Time.unscaledDeltaTime;

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (useThirdPerson)
        {
            // Flatten rotation vectors
            Vector3 flatForward = Vector3.ProjectOnPlane(target.forward, Vector3.up).normalized;
            Vector3 flatRight = Vector3.ProjectOnPlane(target.right, Vector3.up).normalized;

            // Calculate desired offset
            Vector3 desired = target.position
                            + flatRight * thirdPersonOffset.x
                            + flatForward * thirdPersonOffset.z;

            // Smooth XZ only
            Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetXZ = new Vector2(desired.x, desired.z);
            Vector2 smoothedXZ = Vector2.SmoothDamp(currentXZ, targetXZ, ref velocityXZ, positionSmoothTime, Mathf.Infinity, deltaTime);

            float fixedY = target.position.y + thirdPersonOffset.y;
            targetPosition = new Vector3(smoothedXZ.x, fixedY, smoothedXZ.y);

            // Rotation: Yaw only
            if (flatForward.sqrMagnitude < 0.001f)
                flatForward = transform.forward;
            targetRotation = Quaternion.LookRotation(flatForward, Vector3.up);
        }
        else
        {
            // First-person full smoothing
            Vector3 desiredPosition = target.position + target.TransformDirection(firstPersonOffset);
            targetPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity3D, positionSmoothTime, Mathf.Infinity, deltaTime);
            targetRotation = target.rotation;
        }

        // Smooth rotation
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime / rotationSmoothTime);
        transform.position = targetPosition;
        transform.rotation = currentRotation;
    }
}
