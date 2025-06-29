using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

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

    [Header("Waveform Texture Settings")]
    public int waveformTextureWidth = 1024;
    public int waveformTextureHeight = 128;

    [Header("Waveform Visual Settings")]
    public float waveformWidth = 1f;         // Thickness of the waveform
    public float waveformYOffset = 0.01f;    // Slight lift to avoid z-fighting
    public float waveformHorizontalOffset = 0f; // Left/right shift
    public Material waveformMaterial; // Assign a transparent unlit material
    private Texture2D waveformTexture;
    private Mesh waveformMesh;
#if UNITY_EDITOR
    [Header("Waveform Export Settings")]
    public string waveformSavePath = "Assets/Graphics/WaveformTextures/";
    private System.DateTime lastWaveformFileTime = System.DateTime.MinValue;
#endif
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
#if UNITY_EDITOR
        if (musicClip != null && !string.IsNullOrEmpty(waveformSavePath))
        {
            string filePath = System.IO.Path.Combine(waveformSavePath, GeneratedWaveformFileName);

            if (System.IO.File.Exists(filePath))
            {
                System.DateTime fileTime = System.IO.File.GetLastWriteTime(filePath);

                if (fileTime != lastWaveformFileTime)
                {
                    lastWaveformFileTime = fileTime;
                    LoadWaveformTextureFromDisk();
                    GenerateWaveformMesh();
                    Debug.Log($"[Waveform] Regenerated mesh for '{musicClip.name}' due to file change at: {filePath}");
                }
            }
        }
#endif

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

#if UNITY_EDITOR
        if (waveformMaterial == null || waveformMesh == null || waveformTexture == null)
            return;

        waveformMaterial.mainTexture = waveformTexture;
        waveformMaterial.SetPass(0);

        Graphics.DrawMeshNow(waveformMesh, Matrix4x4.identity);
#endif
    }
    private void DrawTimingLines()
{
    if (pathPoints == null || pathPoints.Length != 2 || pathPoints[0] == null || pathPoints[1] == null)
        return;

    Gizmos.color = Color.blue;

    float segmentLength = Vector3.Distance(pathPoints[0].position, pathPoints[1].position);
    float delayOffset = startDelay * speed;

    int totalLines = Mathf.CeilToInt((segmentLength - delayOffset) / timingLineSpacing);

    Vector3 direction = (pathPoints[1].position - pathPoints[0].position).normalized;
    Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x); // left

    for (int i = 0; i <= totalLines; i++)
    {
        float distanceAlongPath = delayOffset + i * timingLineSpacing;
        float t = distanceAlongPath / segmentLength;

        if (t > 1f) break;

        Vector3 pointOnLine = Vector3.Lerp(pathPoints[0].position, pathPoints[1].position, t);
        Vector3 lineStart = pointOnLine + perpendicular * (timingLineWidth / 2);
        Vector3 lineEnd = pointOnLine - perpendicular * (timingLineWidth / 2);

        Gizmos.DrawLine(lineStart, lineEnd);

#if UNITY_EDITOR
        // Use lineEnd (right edge) + slight offset to push it right
        Vector3 labelPos = lineEnd - perpendicular * 0.1f;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.blue;
        style.fontStyle = FontStyle.Bold;

        Handles.Label(labelPos, i.ToString(), style);
#endif
    }
}
    private void ApplyEasyMode()
    {
        if (!easyMode || pathParent == null) return;

        int count = 0;
        List<GameObject> spawnableObjects = new List<GameObject>();

        // Recursively collect spawnable-tagged objects in hierarchy order
        void CollectSpawnablesInOrder(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.CompareTag("Spawnable"))
                {
                    spawnableObjects.Add(child.gameObject);
                }

                // Recurse through children to maintain top-down order
                CollectSpawnablesInOrder(child);
            }
        }

        CollectSpawnablesInOrder(pathParent);

        // Destroy every non-kept object based on interval
        for (int i = 0; i < spawnableObjects.Count; i++)
        {
            if (i % easyModeInterval != 0)
            {
                Destroy(spawnableObjects[i]);
            }
        }
    }

#if UNITY_EDITOR
    public void GenerateWaveformTexture()
    {
        if (musicClip == null) return;

        int width = waveformTextureWidth;
        int height = waveformTextureHeight;

        float[] samples = new float[musicClip.samples * musicClip.channels];
        musicClip.GetData(samples, 0);

        float[] waveform = new float[width];

        int packSize = (samples.Length / width);
        for (int i = 0; i < width; i++)
        {
            float max = 0;
            int start = i * packSize;
            int end = Mathf.Min(start + packSize, samples.Length);

            for (int j = start; j < end; j++)
            {
                float val = Mathf.Abs(samples[j]);
                if (val > max) max = val;
            }

            waveform[i] = max;
        }

        waveformTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        waveformTexture.wrapMode = TextureWrapMode.Clamp;
        waveformTexture.filterMode = FilterMode.Bilinear;

        Color background = new Color(0, 0, 0, 0); // transparent
        Color foreground = Color.cyan;

        for (int x = 0; x < width; x++)
        {
            int yHeight = Mathf.RoundToInt(waveform[x] * height);
            for (int y = 0; y < height; y++)
            {
                waveformTexture.SetPixel(x, y, y < yHeight ? foreground : background);
            }
        }

        waveformTexture.Apply();


        SaveWaveformTexture();
    }
    public void GenerateWaveformMesh()
    {
        if (pathPoints == null || pathPoints.Length != 2 || waveformTexture == null) return;
        if (musicClip == null) return;
        LoadWaveformTextureFromDisk();
        float musicDuration = musicClip.length;

        Vector3 pathDirection = (pathPoints[1].position - pathPoints[0].position).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, pathDirection).normalized;

        // Apply offset shift
        float offset = (startDelay + (manager != null ? manager.musicOffset : 0f)) * speed;

        // Offset to start from the adjusted timing line
        Vector3 start = pathPoints[0].position + pathDirection * offset;
        Vector3 end = pathPoints[0].position + pathDirection * ((musicDuration + startDelay + (manager != null ? manager.musicOffset : 0f)) * speed);

        // Apply user horizontal offset
        start += right * waveformHorizontalOffset;
        end += right * waveformHorizontalOffset;

        Vector3 offsetUp = Vector3.up * waveformYOffset;
        Vector3 halfWidthOffset = right * (waveformWidth / 2f);

        waveformMesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];

        // Aligned to timing line start/end + correct orientation
        vertices[0] = start - halfWidthOffset + offsetUp; // bottom left
        vertices[1] = start + halfWidthOffset + offsetUp; // top left
        vertices[2] = end + halfWidthOffset + offsetUp;   // top right
        vertices[3] = end - halfWidthOffset + offsetUp;   // bottom right

        // Flip Y-axis to match waveform orientation
        uvs[0] = new Vector2(0, 1);
        uvs[1] = new Vector2(0, 0);
        uvs[2] = new Vector2(1, 0);
        uvs[3] = new Vector2(1, 1);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 0;
        triangles[4] = 3;
        triangles[5] = 2;

        waveformMesh.vertices = vertices;
        waveformMesh.uv = uvs;
        waveformMesh.triangles = triangles;
        waveformMesh.RecalculateNormals();
    }



    public void SaveWaveformTexture()
    {
        if (waveformTexture == null)
        {
            Debug.LogWarning("No waveform texture to save.");
            return;
        }

        byte[] pngData = waveformTexture.EncodeToPNG();
        if (pngData == null)
        {
            Debug.LogError("Failed to encode waveform texture to PNG.");
            return;
        }

        string directory = waveformSavePath;
        string texturePath = System.IO.Path.Combine(waveformSavePath, GeneratedWaveformFileName);
        

        if (!System.IO.Directory.Exists(directory))
            System.IO.Directory.CreateDirectory(directory);

        // Save texture PNG
        System.IO.File.WriteAllBytes(texturePath, pngData);
        Debug.Log($"Waveform texture saved to: {texturePath}");

        DuplicateMaterial();

        AssetDatabase.Refresh();
    }

    public void LoadWaveformTextureFromDisk()
    {
        string texturePath = System.IO.Path.Combine(waveformSavePath, GeneratedWaveformFileName);
        string materialPath = GeneratedWaveformMaterialPath;

        if (!System.IO.File.Exists(texturePath))
        {
            Debug.LogWarning("Waveform texture file not found at: " + texturePath);
            return;
        }

        byte[] fileData = System.IO.File.ReadAllBytes(texturePath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(fileData))
        {
            Debug.LogError("Failed to load waveform texture from file.");
            return;
        }

        waveformTexture = tex;
        waveformTexture.wrapMode = TextureWrapMode.Clamp;
        waveformTexture.filterMode = FilterMode.Bilinear;

        Debug.Log("Loaded waveform texture from: " + texturePath);

        // Try loading duplicated material
        Material loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (loadedMaterial != null)
        {
            waveformMaterial = loadedMaterial;
            Debug.Log($"Loaded duplicated waveform material: {materialPath}");
        }
        else
        {
            Debug.LogWarning($"No saved material found at {materialPath}, duplicating material.");
            DuplicateMaterial();
            if (waveformMaterial != null)
                waveformMaterial.mainTexture = waveformTexture;
        }
    }


    private void DuplicateMaterial()
    {
        string materialPath = GeneratedWaveformMaterialPath;
        // Duplicate and save material
        if (waveformMaterial != null)
        {
            Material duplicated = new Material(waveformMaterial);
            duplicated.mainTexture = waveformTexture;

            AssetDatabase.CreateAsset(duplicated, materialPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Waveform material duplicated and saved to: {materialPath}");
        }
    }
   
    public string GeneratedWaveformFileName
    {
        get
        {
            string name = musicClip != null ? musicClip.name : "Unknown";
            return $"Waveform_{name}.png";
        }
    }

    public string GeneratedWaveformMaterialPath
    {
        get
        {
            string name = musicClip != null ? musicClip.name : "Unknown";
            return System.IO.Path.Combine(waveformSavePath, $"Waveform_{name}.mat");
        }
    }

#endif
}
