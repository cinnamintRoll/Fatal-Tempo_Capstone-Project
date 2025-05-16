using UnityEngine;
using System.Collections.Generic;

public class PathFollower : MonoBehaviour
{
    public Transform[] pathPoints;  // Should always be length 2
    public float speed = 5.0f;      // Speed at which to move along the path
    private List<Vector3> samplePoints = new List<Vector3>(); // Precomputed path points
    private int currentPointIndex = 0; // Current point index
    public int sampleCount = 100; // Number of samples along the path
    public float startDelay = 0f; // Delay in seconds before music starts

    public Transform pathParent; // Parent transform for path points

    public AudioClip musicClip; // The music clip to base path length on
    public GameObject spawnTriggerPrefab;
    public float timingLineSpacing; // Calculated spacing for timing lines
    public float bpm = 120f; // Beats per minute
    public float timingLineWidth = 0.1f; // Width of timing lines

    void Start()
    {
        SetupTwoPointPath();
        UpdateSecondPointPosition(); // [MODIFIED]
        ComputeSamplePoints();
        CalculateTimingLineSpacing();
    }

    private void OnValidate()
    {
        if (pathPoints != null && pathPoints.Length == 2)
        {
            UpdateSecondPointPosition(); // [MODIFIED]
            ComputeSamplePoints();
            CalculateTimingLineSpacing();
        }
    }

    void Update()
    {
        if (samplePoints.Count < 2)
        {
            Debug.LogError("You need at least 2 sample points to follow the path.");
            return;
        }

        if (currentPointIndex < samplePoints.Count - 1)
        {
            Vector3 targetPoint = samplePoints[currentPointIndex + 1];
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);

            if (transform.position == targetPoint)
            {
                currentPointIndex++;
            }
        }
    }

    public void SetupTwoPointPath()
    {
        if (pathPoints == null || pathPoints.Length != 2)
        {
            pathPoints = new Transform[2];
        }

        if (pathParent == null)
        {
            GameObject parent = new GameObject("PathPoints");
            parent.transform.parent = transform;
            pathParent = parent.transform;
        }

        if (pathPoints[0] == null)
        {
            GameObject startPoint = new GameObject("Point 0");
            startPoint.transform.parent = pathParent;
            startPoint.transform.position = transform.position;
            pathPoints[0] = startPoint.transform;
        }
        else
        {
            pathPoints[0].position = transform.position;
        }

        if (pathPoints[1] == null)
        {
            GameObject endPoint = new GameObject("Point 1");
            endPoint.transform.parent = pathParent;
            pathPoints[1] = endPoint.transform;
        }

        UpdateSecondPointPosition(); // [MODIFIED]
    }

    private void UpdateSecondPointPosition() // [NEW]
    {
        if (pathPoints == null || pathPoints.Length != 2 || pathPoints[1] == null || pathPoints[0] == null)
            return;

        float pathLength = 10f;

        if (musicClip != null)
        {
            pathLength = musicClip.length * speed;
        }

        pathPoints[1].position = pathPoints[0].position + Vector3.forward * pathLength;
    }

    private void ComputeSamplePoints()
    {
        samplePoints.Clear();

        if (pathPoints == null || pathPoints.Length != 2)
        {
            Debug.LogError("Path must have exactly 2 points.");
            return;
        }

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector3 point = Vector3.Lerp(pathPoints[0].position, pathPoints[1].position, t);
            samplePoints.Add(point);
        }
    }

    private void CalculateTimingLineSpacing()
    {
        float beatDuration = 60f / bpm;
        timingLineSpacing = speed * beatDuration;
    }

    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Length != 2)
            return;

        Gizmos.color = Color.red;

        Vector3 previousPoint = pathPoints[0].position;
        int samples = sampleCount;

        for (int i = 1; i <= samples; i++)
        {
            float t = (float)i / samples;
            Vector3 currentPoint = Vector3.Lerp(pathPoints[0].position, pathPoints[1].position, t);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        DrawTimingLines();
    }

    private void DrawTimingLines()
    {
        Gizmos.color = Color.blue;

        float segmentLength = Vector3.Distance(pathPoints[0].position, pathPoints[1].position);

        // Offset distance from delay
        float delayOffset = startDelay * speed;

        // Start after delayOffset, then every timingLineSpacing
        int totalLines = Mathf.CeilToInt((segmentLength - delayOffset) / timingLineSpacing);

        for (int i = 0; i <= totalLines; i++)
        {
            float distanceAlongPath = delayOffset + i * timingLineSpacing;
            float t = distanceAlongPath / segmentLength;

            if (t > 1f) break; // Avoid overshooting the path

            Vector3 pointOnLine = Vector3.Lerp(pathPoints[0].position, pathPoints[1].position, t);

            Vector3 direction = (pathPoints[1].position - pathPoints[0].position).normalized;
            Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x);

            Vector3 lineStart = pointOnLine + perpendicular * (timingLineWidth / 2);
            Vector3 lineEnd = pointOnLine - perpendicular * (timingLineWidth / 2);

            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }

}
