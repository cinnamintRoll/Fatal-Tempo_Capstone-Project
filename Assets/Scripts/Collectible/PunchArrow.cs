using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Import the UnityEvents namespace

public class PunchArrow : MonoBehaviour
{
    public float idealCollectorDistance = 1.5f; // The ideal distance from the punch arrow
    public float maxTimingScore = 100f; // Maximum score for perfect timing
    public float maxSpeedScore = 100f; // Maximum score for punch speed
    public float maxAngleScore = 100f; // Maximum score for perfect angle
    public float maxDistance = 3.0f; // Maximum distance before it's considered a miss
    public float maxAllowedAngle = 40f; // Maximum angle deviation (in degrees) for a valid hit
    public Transform ObjectVisuals;
    public GameObject Bullethit;
    private bool hitRegistered = false;

    // External transform to represent the desired punch angle
    public Transform punchAngleTransform;

    // Unity Event to trigger on hit
    public UnityEvent onHit;


    private void FixedUpdate()
    {
        if (ObjectVisuals == null)
        {
            Destroy(gameObject);
        }

        if (Bullethit == null)
        {
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (hitRegistered) return; // Prevent multiple hits

        // Check if the other object has a Collector component (not using "Hand" tag or specific player)
        Collector collectorScript = other.GetComponent<Collector>();
        if (collectorScript != null)
        {
            // Calculate angle first; if it's a miss, exit early
            float anglePoints = CalculateAnglePoints(collectorScript);
            if (anglePoints == 0)
            {
                Debug.Log("Hit missed due to wrong angle.");
                hitRegistered = true;
                return; // Early exit as it's a miss
            }

            // Calculate all factors for point calculation
            float speedPoints = CalculateSpeedPoints(other);
            // Call the onHit event
            onHit.Invoke();

            // Sum all points
            float totalPoints = anglePoints + speedPoints;
            Debug.Log($"Hit Successful! Total Points: {totalPoints}");

            // Modify points in PointsManager if it exists
            if (PointsManager.Instance != null)
            {
                PointsManager.Instance.ModifyPoints((int)totalPoints);
            }

            hitRegistered = true; // Prevent multiple hits from being registered
            if (ObjectVisuals == null)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Collector collectorScript = other.GetComponent<Collector>();
        if (collectorScript != null)
        {
            if (hitRegistered)
            {
                if (ObjectVisuals == null)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Collector collectorScript = other.GetComponent<Collector>();
        if (collectorScript != null)
        {
            // Distance from the collector to the punch arrow
            float distanceToCollector = (transform.position - other.transform.position).magnitude;
        }
    }

    private float CalculateTimingPoints(Collider other)
    {
        // Calculate the distance between the collector and the punch arrow
        float distanceToCollector = (transform.position - other.transform.position).magnitude;

        // If the collector is farther than maxDistance, it's a miss and no points are awarded
        if (distanceToCollector > maxDistance)
        {
            Debug.Log("Collector missed the hit (too far).");
            return 0f;
        }

        // The closer the collector is to the ideal distance, the more points they get
        float distanceDifference = Mathf.Abs(distanceToCollector - idealCollectorDistance);

        // Calculate points based on how close the collector is to the ideal distance
        float timingPoints = Mathf.Max(0, maxTimingScore - (distanceDifference / idealCollectorDistance) * maxTimingScore);
        Debug.Log($"Timing Points: {timingPoints}");

        return timingPoints;
    }

    private float CalculateAnglePoints(Collector other)
    {
        // Use the external punch angle transform's forward direction if it's set
        Vector3 punchArrowDirection = punchAngleTransform != null ? punchAngleTransform.forward : transform.forward;

        // Calculate the angle between the hit direction and the punch angle direction
        float angle = Vector3.Angle(other.CollectorVelocity, punchArrowDirection);

        // If the angle is greater than the maxAllowedAngle, it's considered a miss
        if (angle > maxAllowedAngle)
        {
            Debug.Log($"Hit missed due to large angle: {angle} degrees.");
            return 0f; // No points for the wrong angle
        }

        // Normalize angle (0 degrees = max points, close to maxAllowedAngle = no points)
        float anglePoints = Mathf.Max(0, maxAngleScore - (angle / maxAllowedAngle) * maxAngleScore);
        Debug.Log($"Angle Points: {anglePoints}");
        return anglePoints;
    }

    private float CalculateSpeedPoints(Collider other)
    {
        // Speed is based on the velocity magnitude of the collector's Rigidbody at the point of contact
        Rigidbody collectorRigidbody = other.GetComponent<Rigidbody>();
        if (collectorRigidbody == null) return 0;

        float hitSpeed = collectorRigidbody.velocity.magnitude;

        // Normalize speed to give more points for higher speeds
        float maxSpeedThreshold = 10f;
        float speedPoints = Mathf.Min(maxSpeedScore, (hitSpeed / maxSpeedThreshold) * maxSpeedScore);
        Debug.Log($"Speed Points: {speedPoints}");
        return speedPoints;
    }
}
