using UnityEngine;
using System.Collections.Generic;

public class PathFollower : MonoBehaviour
{
    public Transform[] pathPoints;  // Array of waypoints
    public float speed = 5.0f;      // Speed at which to move along the path
    private List<Vector3> samplePoints = new List<Vector3>(); // Precomputed path points
    private int currentPointIndex = 0; // Current point index
    public int sampleCount = 100; // Number of samples along the path

    public Transform pathParent; // Parent transform for path points
    public float bpm = 120f; // Beats per minute
    public float timingLineWidth = 0.1f; // Width of timing lines

    public float timingLineSpacing; // Calculated spacing for timing lines
    public bool enableCurving = true; // Toggle for curving

    void Start()
    {
        //CreatePathParent();
        ComputeSamplePoints();
        CalculateTimingLineSpacing();
    }

    private void OnValidate()
    {
        // Recalculate sample points and timing line spacing when parameters change in the Inspector
        if (pathPoints != null && pathPoints.Length > 1)
        {
            ComputeSamplePoints();
            CalculateTimingLineSpacing();
        }
    }

    void Update()
    {
        if (samplePoints.Count < 2)
        {
            Debug.LogError("You need at least 2 points to follow the path.");
            return;
        }

        // Move towards the next sample point
        if (currentPointIndex < samplePoints.Count - 1)
        {
            Vector3 targetPoint = samplePoints[currentPointIndex + 1];
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);

            // Check if we reached the target point
            if (transform.position == targetPoint)
            {
                currentPointIndex++;
            }
        }
    }

    private void CreatePathParent()
    {
        if (pathParent == null)
        {
            GameObject parent = new GameObject("PathPoints");
            parent.transform.parent = transform; // Set the path points as children of this object
            pathParent = parent.transform; // Store the parent for future reference
        }

        // Automatically populate pathPoints with the children of pathParent
        List<Transform> children = new List<Transform>();

        foreach (Transform child in pathParent)
        {
            children.Add(child); // Add each child of pathParent to the list
        }

        pathPoints = children.ToArray(); // Convert list to array and assign it to pathPoints
    }


    private void ComputeSamplePoints()
    {
        samplePoints.Clear(); // Clear any previous sample points
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            int segmentSamples = sampleCount / (pathPoints.Length - 1);
            for (int j = 0; j <= segmentSamples; j++)
            {
                float t = (float)j / segmentSamples;
                Vector3 point;

                if (enableCurving)
                {
                    point = CatmullRom(
                        GetControlPoint(i - 1),
                        pathPoints[i].position,
                        pathPoints[i + 1].position,
                        GetControlPoint(i + 2),
                        t
                    );
                }
                else
                {
                    point = Vector3.Lerp(pathPoints[i].position, pathPoints[i + 1].position, t);
                }

                samplePoints.Add(point);
            }
        }
    }

    private void CalculateTimingLineSpacing()
    {
        // Calculate the duration of one beat in seconds
        float beatDuration = 60f / bpm;

        // Calculate the spacing based on the speed and beat duration
        timingLineSpacing = speed * beatDuration; // Adjust this calculation based on your requirements
    }

    public Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }

    public Vector3 GetControlPoint(int index)
    {
        if (index < 0)
            return pathPoints[0].position;
        if (index >= pathPoints.Length)
            return pathPoints[pathPoints.Length - 1].position;

        return pathPoints[index].position;
    }

    private void OnDrawGizmos()
    {
        if (pathPoints != null && pathPoints.Length >= 2)
        {
            Gizmos.color = Color.red;

            // Draw Catmull-Rom splines between the points
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                Vector3 previousPosition = pathPoints[i].position;
                int segmentSamples = sampleCount / (pathPoints.Length - 1);
                for (int j = 0; j <= segmentSamples; j++)
                {
                    float t = (float)j / segmentSamples;
                    Vector3 point;

                    if (enableCurving)
                    {
                        point = CatmullRom(
                            GetControlPoint(i - 1),
                            pathPoints[i].position,
                            pathPoints[i + 1].position,
                            GetControlPoint(i + 2),
                            t
                        );
                    }
                    else
                    {
                        point = Vector3.Lerp(pathPoints[i].position, pathPoints[i + 1].position, t);
                    }

                    Gizmos.DrawLine(previousPosition, point);
                    previousPosition = point;
                }
            }

            // Draw debug timing lines
            DrawTimingLines();
        }
    }

    private void DrawTimingLines()
    {
        Gizmos.color = Color.blue; // Change color for timing lines

        // Iterate over the segments
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            float segmentLength = Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
            int samples = Mathf.CeilToInt(segmentLength / timingLineSpacing);

            for (int j = 0; j <= samples; j++)
            {
                float t = (float)j / samples;
                Vector3 pointOnCurve;

                if (enableCurving)
                {
                    pointOnCurve = CatmullRom(
                        GetControlPoint(i - 1),
                        pathPoints[i].position,
                        pathPoints[i + 1].position,
                        GetControlPoint(i + 2),
                        t
                    );
                }
                else
                {
                    pointOnCurve = Vector3.Lerp(pathPoints[i].position, pathPoints[i + 1].position, t);
                }

                // Calculate direction along the curve
                Vector3 nextPointOnCurve;

                if (enableCurving)
                {
                    nextPointOnCurve = CatmullRom(
                        GetControlPoint(i - 1),
                        pathPoints[i].position,
                        pathPoints[i + 1].position,
                        GetControlPoint(i + 2),
                        Mathf.Min(t + (1f / samples), 1f)
                    );
                }
                else
                {
                    nextPointOnCurve = Vector3.Lerp(pathPoints[i].position, pathPoints[i + 1].position, Mathf.Min(t + (1f / samples), 1f));
                }

                // Calculate the direction vector and the perpendicular vector
                Vector3 direction = (nextPointOnCurve - pointOnCurve).normalized;
                Vector3 perpendicularDirection = new Vector3(-direction.z, 0, direction.x); // Perpendicular vector

                // Draw the centered perpendicular line
                Vector3 lineStart = pointOnCurve + perpendicularDirection * (timingLineWidth / 2);
                Vector3 lineEnd = pointOnCurve - perpendicularDirection * (timingLineWidth / 2);
                Gizmos.DrawLine(lineStart, lineEnd); // Draw the line centered along the path
            }
        }
    }
}