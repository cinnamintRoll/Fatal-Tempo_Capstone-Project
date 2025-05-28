using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathFollower))]
public class PathFollowerEditor : Editor
{
    private PathFollower pathFollower;

    private void OnEnable()
    {
        pathFollower = (PathFollower)target;

        // Ensure pathPoints array has exactly 2 points
        if (pathFollower.pathPoints == null || pathFollower.pathPoints.Length != 2)
        {
            pathFollower.pathPoints = new Transform[2];

            // Create parent if missing
            if (pathFollower.pathParent == null)
            {
                GameObject parent = new GameObject("PathPoints");
                parent.transform.parent = pathFollower.transform;
                pathFollower.pathParent = parent.transform;
            }

            // Create or assign the two points if null
            for (int i = 0; i < 2; i++)
            {
                if (pathFollower.pathPoints[i] == null)
                {
                    GameObject pointGO = new GameObject($"Point {i}");
                    pointGO.transform.parent = pathFollower.pathParent;
                    pointGO.transform.localPosition = Vector3.forward * i * 10f; // default spacing
                    pathFollower.pathPoints[i] = pointGO.transform;
                }
            }

            EditorUtility.SetDirty(pathFollower);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Spawn Triggers"))
        {
            GenerateSpawnTriggers();
        }

        if (GUILayout.Button("Align Beats to Points"))
        {
            AlignBeatsToPoints();
        }

        if (GUILayout.Button("Reshuffle Spawners"))
        {
            ReshuffleSpawners();
        }
    }

    private void GenerateSpawnTriggers()
    {
        if (pathFollower.pathPoints == null || pathFollower.pathPoints.Length != 2) return;

        if (pathFollower.spawnTriggerPrefab == null)
        {
            Debug.LogError("Spawn Trigger Prefab is not assigned in PathFollower.");
            return;
        }

        Vector3 startPoint = pathFollower.pathPoints[0].position;
        Vector3 endPoint = pathFollower.pathPoints[1].position;

        float segmentLength = Vector3.Distance(startPoint, endPoint);
        int samples = Mathf.CeilToInt(segmentLength / pathFollower.timingLineSpacing);

        Vector3 pathDirection = (endPoint - startPoint).normalized;
        Vector3 perpendicular = Vector3.Cross(pathDirection, Vector3.up).normalized;

        for (int j = 0; j <= samples; j++)
        {
            float t = (float)j / samples;
            Vector3 pointOnLine = Vector3.Lerp(startPoint, endPoint, t);

            // Instantiate spawn trigger prefab
            GameObject spawnTrigger = (GameObject)PrefabUtility.InstantiatePrefab(pathFollower.spawnTriggerPrefab);
            spawnTrigger.transform.position = pointOnLine;
            spawnTrigger.transform.parent = pathFollower.pathParent;

            // Call RandomlyPickSpawn if GeneralSpawner is present
            GeneralSpawner spawner = spawnTrigger.GetComponent<GeneralSpawner>();
            if (spawner != null)
            {
                spawner.RandomlyPickSpawn();
            }
            else
            {
                Debug.LogWarning("Spawn trigger prefab does not have a GeneralSpawner component.");
            }

            Undo.RegisterCreatedObjectUndo(spawnTrigger, "Create Spawn Trigger");
        }

        EditorUtility.SetDirty(pathFollower);
    }

    private void ReshuffleSpawners()
    {
        if (pathFollower.pathParent == null) return;

        foreach (Transform child in pathFollower.pathParent)
        {
            GeneralSpawner spawner = child.GetComponent<GeneralSpawner>();
            if (spawner != null)
            {
                Undo.RecordObject(spawner, "Reshuffle Spawner");
                spawner.RandomlyPickSpawn();
            }
        }

        EditorUtility.SetDirty(pathFollower);
    }

    void OnSceneGUI()
    {
        if (pathFollower.pathPoints == null || pathFollower.pathPoints.Length != 2) return;

        EditorGUI.BeginChangeCheck();

        // Allow moving the two points with position handles
        for (int i = 0; i < 2; i++)
        {
            if (pathFollower.pathPoints[i] != null)
            {
                Vector3 newPos = Handles.PositionHandle(pathFollower.pathPoints[i].position, Quaternion.identity);
                if (newPos != pathFollower.pathPoints[i].position)
                {
                    Undo.RecordObject(pathFollower.pathPoints[i], "Move Path Point");
                    pathFollower.pathPoints[i].position = newPos;
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(pathFollower);
        }
    }

    private void AlignBeatsToPoints()
    {
        if (pathFollower.pathPoints == null || pathFollower.pathPoints.Length != 2) return;

        Vector3 startPoint = pathFollower.pathPoints[0].position;
        Vector3 endPoint = pathFollower.pathPoints[1].position;
        Vector3 pathDirection = (endPoint - startPoint).normalized;

        float segmentLength = Vector3.Distance(startPoint, endPoint);
        float spacing = pathFollower.timingLineSpacing;
        float startOffset = pathFollower.startDelay * pathFollower.speed;
        int totalChildren = pathFollower.pathParent.childCount;

        for (int i = 0; i < totalChildren; i++)
        {
            Transform child = pathFollower.pathParent.GetChild(i);
            float beatDistance = startOffset + i * spacing;
            float t = Mathf.Clamp01(beatDistance / segmentLength);
            Vector3 alignedPos = Vector3.Lerp(startPoint, endPoint, t);

            Undo.RecordObject(child, "Align Beat to Point");
            child.position = alignedPos;
        }

        EditorUtility.SetDirty(pathFollower);
    }
}
