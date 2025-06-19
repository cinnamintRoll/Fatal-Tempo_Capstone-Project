using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static EnemyAI;

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
    if (GUILayout.Button("Place End Trigger at End Point"))
    {
        PlaceEndTriggerIfMissing();
    }
        if (GUILayout.Button("Re-show All Spawner Visuals"))
        {
            ReShowAllSpawnerVisuals();
        }

        if (GUILayout.Button("Replace All Spawnables with Prefab"))
        {
            ReplaceSpawnablesWithCurrentPrefab();
        }

    }

    private void ReShowAllSpawnerVisuals()
    {
        if (pathFollower.pathParent == null)
        {
            Debug.LogWarning("PathFollower: pathParent is not assigned, cannot re-show spawner visuals.");
            return;
        }

        int count = 0;

        void TraverseAndReshow(Transform parent)
        {
            foreach (Transform child in parent)
            {
                TraverseAndReshow(child); // Go deeper

                GeneralSpawner spawner = child.GetComponent<GeneralSpawner>();
                if (spawner != null)
                {
                    spawner.ReShowAllSpawnerVisuals();
                    count++;
                }
            }
        }

        TraverseAndReshow(pathFollower.pathParent);

        Debug.Log($"Re-showed visuals for {count} GeneralSpawner(s) under PathParent.");
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

        GameObject lastTrigger = null;

        for (int j = 0; j <= samples; j++)
        {
            float t = (float)j / samples;
            Vector3 pointOnLine = Vector3.Lerp(startPoint, endPoint, t);

            GameObject spawnTrigger = (GameObject)PrefabUtility.InstantiatePrefab(pathFollower.spawnTriggerPrefab);
            spawnTrigger.transform.position = pointOnLine;
            spawnTrigger.transform.parent = pathFollower.pathParent;

            // Only call RandomlyPickSpawn if it's not the last one
            if (j < samples)
            {
                GeneralSpawner spawner = spawnTrigger.GetComponent<GeneralSpawner>();
                if (spawner != null)
                {
                    spawner.RandomlyPickSpawn();
                }
                else
                {
                    Debug.LogWarning("Spawn trigger prefab does not have a GeneralSpawner component.");
                }
            }

            Undo.RegisterCreatedObjectUndo(spawnTrigger, "Create Spawn Trigger");
            lastTrigger = spawnTrigger;
        }

        // Replace the last trigger with the EndTriggerPrefab
        if (pathFollower.EndTriggerPrefab != null && lastTrigger != null)
        {
            Vector3 pos = lastTrigger.transform.position;
            Quaternion rot = lastTrigger.transform.rotation;

            Undo.DestroyObjectImmediate(lastTrigger);

            GameObject endTrigger = (GameObject)PrefabUtility.InstantiatePrefab(pathFollower.EndTriggerPrefab);
            endTrigger.transform.position = pos;
            endTrigger.transform.rotation = rot;
            endTrigger.transform.parent = pathFollower.pathParent;

            Undo.RegisterCreatedObjectUndo(endTrigger, "Create End Trigger");
        }

        EditorUtility.SetDirty(pathFollower);
    }


    private void ReshuffleSpawners()
    {
        if (pathFollower.pathParent == null) return;

        void TraverseAndReshuffle(Transform parent)
        {
            foreach (Transform child in parent)
            {
                TraverseAndReshuffle(child); // Go deeper

                GeneralSpawner spawner = child.GetComponent<GeneralSpawner>();
                if (spawner != null)
                {
                    Undo.RecordObject(spawner, "Reshuffle Spawner");
                    spawner.RandomlyPickSpawn();
                }
            }
        }

        TraverseAndReshuffle(pathFollower.pathParent);

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

    private void PlaceEndTriggerIfMissing()
    {
        if (pathFollower.EndTriggerPrefab == null)
        {
            Debug.LogError("EndTriggerPrefab is not assigned.");
            return;
        }

        if (pathFollower.pathPoints == null || pathFollower.pathPoints.Length < 2 || pathFollower.pathPoints[1] == null)
        {
            Debug.LogError("Path endpoint is not properly defined.");
            return;
        }

        Vector3 endPosition = pathFollower.pathPoints[1].position;
        float checkRadius = 0.1f;

        // Check if any existing EndTrigger is already close to the endpoint
        foreach (Transform child in pathFollower.pathParent)
        {
            if (child.name.Contains(pathFollower.EndTriggerPrefab.name) && Vector3.Distance(child.position, endPosition) <= checkRadius)
            {
                Debug.Log("An End Trigger already exists at the end point.");
                return;
            }
        }

        // Instantiate the EndTriggerPrefab
        GameObject endTrigger = (GameObject)PrefabUtility.InstantiatePrefab(pathFollower.EndTriggerPrefab);
        endTrigger.transform.position = endPosition;
        endTrigger.transform.rotation = Quaternion.identity;
        endTrigger.transform.parent = pathFollower.pathParent;

        Undo.RegisterCreatedObjectUndo(endTrigger, "Place End Trigger");
        EditorUtility.SetDirty(pathFollower);
    }

    private void AlignBeatsToPoints()
    {
        if (pathFollower.pathPoints == null || pathFollower.pathPoints.Length != 2) return;

        Vector3 startPoint = pathFollower.pathPoints[0].position;
        Vector3 endPoint = pathFollower.pathPoints[1].position;
        float segmentLength = Vector3.Distance(startPoint, endPoint);
        float spacing = pathFollower.timingLineSpacing;
        float startOffset = pathFollower.startDelay * pathFollower.speed;
        int totalChildren = pathFollower.pathParent.childCount;

        if (!pathFollower.useClosestPointAlignment)
        {
            // Original linear alignment
            Vector3 pathDirection = (endPoint - startPoint).normalized;
            for (int i = 0; i < totalChildren; i++)
            {
                Transform child = pathFollower.pathParent.GetChild(i);
                if (child.tag != "Spawnable" || child.tag != "EndSaver") continue;

                float beatDistance = startOffset + i * spacing;
                float t = Mathf.Clamp01(beatDistance / segmentLength);
                Vector3 alignedPos = Vector3.Lerp(startPoint, endPoint, t);

                Undo.RecordObject(child, "Align Beat to Point");
                child.position = alignedPos;
            }
        }
        else
        {
            // Closest point alignment

            // Find or create the parent in scene root
            GameObject alignmentParent = GameObject.Find("ClosestPointAlignmentParent");
            if (alignmentParent == null)
            {
                alignmentParent = new GameObject("ClosestPointAlignmentParent");
                Undo.RegisterCreatedObjectUndo(alignmentParent, "Create Closest Point Alignment Parent");
            }

            // Clear existing children in alignmentParent
            for (int i = alignmentParent.transform.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(alignmentParent.transform.GetChild(i).gameObject);
            }

            // Calculate how many points needed along the line
            int pointsCount = Mathf.CeilToInt(segmentLength / spacing) + 1;

            // Create empty transforms spaced along the line, starting at startOffset
            for (int i = 0; i < pointsCount; i++)
            {
                float beatDistance = startOffset + i * spacing;
                float tWithOffset = Mathf.Clamp01(beatDistance / segmentLength);
                Vector3 pos = Vector3.Lerp(startPoint, endPoint, tWithOffset);

                GameObject pointGO = new GameObject($"AlignPoint {i}");
                Undo.RegisterCreatedObjectUndo(pointGO, "Create Alignment Point");
                pointGO.transform.position = pos;
                pointGO.transform.parent = alignmentParent.transform;
            }

            // For each child in pathParent, find closest align point and assign
            foreach (Transform child in pathFollower.pathParent)
            {
                if (child.tag != "Spawnable" || child.tag != "EndSaver") continue;

                Transform closest = null;
                float closestDist = float.MaxValue;

                foreach (Transform alignPoint in alignmentParent.transform)
                {
                    float dist = Vector3.Distance(child.position, alignPoint.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = alignPoint;
                    }
                }

                if (closest != null)
                {
                    Undo.RecordObject(child, "Align Beat to Closest Point");
                    child.position = closest.position;
                }
            }

            Undo.DestroyObjectImmediate(alignmentParent);
        }

        EditorUtility.SetDirty(pathFollower);
    }

    private void ReplaceSpawnablesWithCurrentPrefab()
    {
        if (pathFollower.spawnTriggerPrefab == null)
        {
            Debug.LogError("Spawn Trigger Prefab is not assigned.");
            return;
        }

        int replacedCount = 0;

        // Recursive function to traverse all children
        void TraverseAndReplace(Transform parent)
        {
            foreach (Transform child in parent)
            {
                // First, go deeper
                TraverseAndReplace(child);

                GeneralSpawner oldSpawner = child.GetComponent<GeneralSpawner>();
                if (oldSpawner == null) continue;

                GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (prefabSource == pathFollower.spawnTriggerPrefab)
                    continue; // Already correct prefab

                // Store data
                Vector3 pos = child.position;
                Quaternion rot = child.rotation;
                string name = child.name;
                Transform originalParent = child.parent;
                int siblingIndex = child.GetSiblingIndex();

                int oldSpawnIndex = oldSpawner.spawnIndex;
                Vector3? spawnPointPos = oldSpawner.spawnPoint ? oldSpawner.spawnPoint.position : null;
                Vector3? movePointPos = oldSpawner.movepoint ? oldSpawner.movepoint.position : null;
                string selectedEnemyName = oldSpawner.enemy ? oldSpawner.enemy.selectedEnemyName : null;
                EnemyAI oldEnemyAI = oldSpawner.enemy;
                EnemyState currentState = oldEnemyAI ? oldEnemyAI.currentState : EnemyState.Idle;

                Undo.DestroyObjectImmediate(child.gameObject);

                GameObject newGO = (GameObject)PrefabUtility.InstantiatePrefab(pathFollower.spawnTriggerPrefab);
                newGO.name = name;
                newGO.transform.position = pos;
                newGO.transform.rotation = rot;
                newGO.transform.SetParent(originalParent);
                newGO.transform.SetSiblingIndex(siblingIndex); // Maintain original order

                GeneralSpawner newSpawner = newGO.GetComponent<GeneralSpawner>();
                if (newSpawner != null)
                {
                    newSpawner.spawnIndex = oldSpawnIndex;

                    if (newSpawner.spawnPoint && spawnPointPos.HasValue)
                        newSpawner.spawnPoint.position = spawnPointPos.Value;

                    if (newSpawner.movepoint && movePointPos.HasValue)
                        newSpawner.movepoint.position = movePointPos.Value;

                    if (newSpawner.enemy)
                        newSpawner.enemy.selectedEnemyName = selectedEnemyName;
                        newSpawner.enemy.currentState = currentState;
                }

                Undo.RegisterCreatedObjectUndo(newGO, "Replace Spawnable");
                replacedCount++;
            }
        }

        TraverseAndReplace(pathFollower.pathParent);

        ReShowAllSpawnerVisuals();
        Debug.Log($"Replaced {replacedCount} spawnable(s) with current prefab.");
        EditorUtility.SetDirty(pathFollower);
    }




}
