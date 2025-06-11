using UnityEngine;
using System.Collections.Generic;

public class PathFollower : MonoBehaviour
{
    public Transform[] pathPoints;  // Should always be length 2
    public float speed = 5.0f;      // Speed at which to move along the path
    public List<Vector3> samplePoints = new List<Vector3>(); // Precomputed path points
    private int currentPointIndex = 0; // Current point index
    public int sampleCount = 100; // Number of samples along the path
    public float startDelay = 0f; // Delay in seconds before music starts
    [SerializeField] private MusicManager manager;
    public Transform pathParent; // Parent transform for path points
    
    public AudioClip musicClip; // The music clip to base path length on
    public GameObject spawnTriggerPrefab;
    public GameObject EndTriggerPrefab;
    public float timingLineSpacing; // Calculated spacing for timing lines
    public float bpm = 120f; // Beats per minute
    public float timingLineWidth = 0.1f; // Width of timing lines
    private Vector3 lastStartPos;
    private Vector3 lastEndPos;
    private float lastSampleStartPos;
    private float lastSampleEndPos;
    private int lastSampleCount;
    private float lastMusicLength;
    private float lastSpeed;
    private float lastBpm;

    [Header("Easy Mode Settings")]
    public bool easyMode = false; // Toggle to enable Easy Mode
    [Range(1, 10)]
    public int easyModeInterval = 2; // Keep every Nth spawnable
    public bool useClosestPointAlignment;
    void Start()
    {

        //SetupTwoPointPath();
        //UpdateSecondPointPosition(); 
        //ComputeSamplePoints();
        //CalculateTimingLineSpacing();
        easyMode = PlayerPrefs.GetInt("EasyMode", 0) == 1;
        ApplyEasyMode();

        SetStartFromPosition();
    }

    private void OnValidate()
    {
        if (manager != null)
        {
            if (musicClip != manager.musicClip)
            {
                musicClip = manager.musicClip;
                bpm = manager.bpm;
            }
        }
        if (pathPoints != null && pathPoints.Length == 2 && pathPoints[0] != null && pathPoints[1] != null)
        {
            UpdateSecondPointPosition();
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
        //Debug.Log($"{currentPointIndex}/{samplePoints.Count}");
        if (currentPointIndex < samplePoints.Count - 1)
        {
            Vector3 targetPoint = samplePoints[currentPointIndex + 1];
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPoint) < 0.01f)
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

    private void UpdateSecondPointPosition()
    {
        if (pathPoints == null || pathPoints.Length != 2 || pathPoints[1] == null || pathPoints[0] == null)
            return;

        float pathLength = 10f;

        if (musicClip != null)
        {
            pathLength = musicClip.length * speed;
        }

        if (Mathf.Approximately(lastMusicLength, musicClip != null ? musicClip.length : 0f) &&
            Mathf.Approximately(lastSpeed, speed))
            return;

        pathPoints[1].position = pathPoints[0].position + Vector3.forward * pathLength;

        lastMusicLength = musicClip != null ? musicClip.length : 0f;
        lastSpeed = speed;
    }


    private void ComputeSamplePoints()
    {
        if (pathPoints == null || pathPoints.Length != 2)
        {
            Debug.LogError("Path must have exactly 2 points.");
            return;
        }

        Vector3 startPos = pathPoints[0].position;
        Vector3 endPos = pathPoints[1].position;

        if (startPos == lastStartPos && endPos == lastEndPos && sampleCount == lastSampleCount)
            return;

        samplePoints.Clear();

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector3 point = Vector3.Lerp(startPos, endPos, t);
            samplePoints.Add(point);
        }

        lastStartPos = startPos;
        lastEndPos = endPos;
        lastSampleCount = sampleCount;
    }

    private void SetStartFromPosition()
    {
        if (samplePoints == null || samplePoints.Count < 2 || manager == null || manager.musicSource == null)
            return;

        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        // Find closest point in samplePoints to current position
        for (int i = 0; i < samplePoints.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, samplePoints[i]);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestIndex = i;
            }
        }

        currentPointIndex = closestIndex;

        // Calculate t value (0 to 1) along the path
        float t = (float)closestIndex / (samplePoints.Count - 1);

        // Convert t to time in music
        float musicLength = musicClip != null ? musicClip.length : 0f;
        float time = t * musicLength;

        // Adjust for start delay
        time -= startDelay;
        time = Mathf.Clamp(time, 0f, musicLength);

        // Seek music
        manager.startAtTime = time;
    }

    private void CalculateTimingLineSpacing()
    {
        if (Mathf.Approximately(lastBpm, bpm) && Mathf.Approximately(lastSpeed, speed))
            return;

        float beatDuration = 60f / bpm;
        timingLineSpacing = speed * beatDuration;

        lastBpm = bpm;
        lastSpeed = speed;
    }


    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Length != 2 || pathPoints[0] == null || pathPoints[1] == null)
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

    private void ApplyEasyMode()
    {
        if (!easyMode || pathParent == null) return;

        int count = 0;
        // Use a temp list to avoid modifying the hierarchy while iterating
        List<Transform> children = new List<Transform>();
        foreach (Transform child in pathParent)
        {
            if (child != pathPoints[0] && child != pathPoints[1])
                children.Add(child);
        }

        foreach (Transform child in children)
        {
            bool keep = count % easyModeInterval == 0;
            if (!keep)
                Destroy(child.gameObject);
            count++;
        }
    }
}
