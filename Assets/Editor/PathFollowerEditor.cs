using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(PathFollower))]
public class PathFollowerEditor : Editor
{
    private PathFollower pathFollower;
    private bool isEditing = false;
    private bool isStraightening = false;

    private void OnEnable()
    {
        pathFollower = (PathFollower)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Add buttons to enable editing and straightening
        if (GUILayout.Button(isEditing ? "Stop Editing" : "Edit Path"))
        {
            isEditing = !isEditing;
        }

        if (GUILayout.Button(isStraightening ? "Stop Straightening" : "Straighten Path"))
        {
            isStraightening = !isStraightening;
        }

        // Add "Add New Point" button
        if (GUILayout.Button("Add New Point"))
        {
            AddNewPointAtEnd();
        }

        // Add "Generate Spawn Triggers" button
        if (GUILayout.Button("Generate Spawn Triggers"))
        {
            GenerateSpawnTriggers();
        }
    }

    private void GenerateSpawnTriggers()
    {
        if (pathFollower.pathPoints == null || pathFollower.pathPoints.Length < 2) return;

        for (int i = 0; i < pathFollower.pathPoints.Length - 1; i++)
        {
            float segmentLength = Vector3.Distance(pathFollower.pathPoints[i].position, pathFollower.pathPoints[i + 1].position);
            int samples = Mathf.CeilToInt(segmentLength / pathFollower.timingLineSpacing); // Assuming timingLineSpacing is accessible

            for (int j = 0; j <= samples; j++)
            {
                float t = (float)j / samples;
                Vector3 pointOnCurve;

                if (pathFollower.enableCurving) // Assuming enableCurving is accessible
                {
                    pointOnCurve = pathFollower.CatmullRom(
                        pathFollower.GetControlPoint(i - 1),
                        pathFollower.pathPoints[i].position,
                        pathFollower.pathPoints[i + 1].position,
                        pathFollower.GetControlPoint(i + 2),
                        t
                    );
                }
                else
                {
                    pointOnCurve = Vector3.Lerp(pathFollower.pathPoints[i].position, pathFollower.pathPoints[i + 1].position, t);
                }

                // Create a new empty GameObject at the timing line position
                GameObject spawnTrigger = new GameObject("SpawnTrigger");
                spawnTrigger.transform.position = pointOnCurve; // Position at the timing line
                spawnTrigger.transform.parent = pathFollower.pathParent; // Set parent for organization

                // Optionally, add a component or setup as needed
                spawnTrigger.AddComponent<BoxCollider>().isTrigger = true; // Example: adding a trigger collider
            }
        }

        EditorUtility.SetDirty(pathFollower);
    }



    void OnSceneGUI()
    {
        if (pathFollower.pathPoints == null || pathFollower.pathPoints.Length < 2) return;

        // Begin change tracking
        EditorGUI.BeginChangeCheck();

        // Handle point manipulation and path editing
        for (int i = 0; i < pathFollower.pathPoints.Length; i++)
        {
            if (pathFollower.pathPoints[i] != null)
            {
                Vector3 newPosition = Handles.PositionHandle(pathFollower.pathPoints[i].position, Quaternion.identity);
                if (pathFollower.pathPoints[i].position != newPosition)
                {
                    Undo.RecordObject(pathFollower.pathPoints[i], "Move Path Point");
                    pathFollower.pathPoints[i].position = newPosition;
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(pathFollower);
        }

        // Toggle the editing mode
        if (isEditing)
        {
            HandlePathEditing();
        }

        // Handle straightening if the user toggled that mode
        if (isStraightening)
        {
            HandleStraightening();
        }
    }

    private void AddNewPointAtEnd()
    {
        // Get the last point's position, or default to a point in front of the object
        Vector3 newPointPosition;
        if (pathFollower.pathPoints != null && pathFollower.pathPoints.Length > 0)
        {
            newPointPosition = pathFollower.pathPoints[pathFollower.pathPoints.Length - 1].position + Vector3.forward * 2f;
        }
        else
        {
            newPointPosition = pathFollower.transform.position + Vector3.forward * 2f;
        }

        // Add a new point to the array
        Undo.RecordObject(pathFollower, "Add Path Point");
        Array.Resize(ref pathFollower.pathPoints, pathFollower.pathPoints.Length + 1);

        // Create a parent GameObject if it doesn't exist
        if (pathFollower.pathParent == null)
        {
            GameObject parent = new GameObject("PathPoints");
            parent.transform.parent = pathFollower.transform;
            pathFollower.pathParent = parent.transform; // Store the parent for future reference
        }

        // Create a new point under the parent
        Transform newPoint = new GameObject($"Point {pathFollower.pathPoints.Length}").transform;
        newPoint.position = newPointPosition;
        newPoint.parent = pathFollower.pathParent; // Set the parent of the new point

        // Assign the new point to the path
        pathFollower.pathPoints[pathFollower.pathPoints.Length - 1] = newPoint;
        EditorUtility.SetDirty(pathFollower);
    }

    private void HandlePathEditing()
    {
        Event e = Event.current;

        // Left-click to add new point along the line
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if we clicked near an existing segment to add a new point in the middle
                bool pointAddedInSegment = false;
                for (int i = 0; i < pathFollower.pathPoints.Length - 1; i++)
                {
                    Vector3 startPoint = pathFollower.pathPoints[i].position;
                    Vector3 endPoint = pathFollower.pathPoints[i + 1].position;

                    // Check if the hit point is near the line between two points
                    if (IsPointNearLine(hit.point, startPoint, endPoint, 1f))
                    {
                        // Add a new point in the middle of this segment
                        Undo.RecordObject(pathFollower, "Add Path Point");
                        Array.Resize(ref pathFollower.pathPoints, pathFollower.pathPoints.Length + 1);

                        // Insert the new point between the current and the next point
                        Transform newPoint = new GameObject($"Point {pathFollower.pathPoints.Length}").transform;
                        newPoint.position = (startPoint + endPoint) / 2;
                        newPoint.parent = pathFollower.pathParent; // Set parent

                        // Shift the array to insert the new point
                        for (int j = pathFollower.pathPoints.Length - 1; j > i + 1; j--)
                        {
                            pathFollower.pathPoints[j] = pathFollower.pathPoints[j - 1];
                        }
                        pathFollower.pathPoints[i + 1] = newPoint;
                        e.Use();
                        pointAddedInSegment = true;
                        break;
                    }
                }

                // If no point was added in a segment, add the new point at the hit location (for adding at the end of the path)
                if (!pointAddedInSegment)
                {
                    Undo.RecordObject(pathFollower, "Add Path Point");
                    Array.Resize(ref pathFollower.pathPoints, pathFollower.pathPoints.Length + 1);

                    // Create a parent GameObject if it doesn't exist
                    if (pathFollower.pathParent == null)
                    {
                        GameObject parent = new GameObject("PathPoints");
                        parent.transform.parent = pathFollower.transform;
                        pathFollower.pathParent = parent.transform; // Store the parent for future reference
                    }

                    // Create new point under the parent at the hit location
                    Transform newPoint = new GameObject($"Point {pathFollower.pathPoints.Length}").transform;
                    newPoint.position = hit.point;
                    newPoint.parent = pathFollower.pathParent; // Set the parent of the new point
                    pathFollower.pathPoints[pathFollower.pathPoints.Length - 1] = newPoint;
                    e.Use();
                }
            }
        }

        // Right-click to delete points
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                for (int i = 0; i < pathFollower.pathPoints.Length; i++)
                {
                    if (Vector3.Distance(pathFollower.pathPoints[i].position, hit.point) < 1f)
                    {
                        Undo.RecordObject(pathFollower, "Delete Path Point");
                        var point = pathFollower.pathPoints[i];
                        DestroyImmediate(point.gameObject);
                        pathFollower.pathPoints = RemovePoint(pathFollower.pathPoints, i);
                        e.Use();
                        break;
                    }
                }
            }
        }
    }

    private void HandleStraightening()
    {
        Handles.color = Color.yellow;

        for (int i = 0; i < pathFollower.pathPoints.Length - 1; i++)
        {
            // Draw a line between points
            Handles.DrawLine(pathFollower.pathPoints[i].position, pathFollower.pathPoints[i + 1].position);

            // Add a button to straighten the segment
            if (Handles.Button((pathFollower.pathPoints[i].position + pathFollower.pathPoints[i + 1].position) / 2, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap))
            {
                Undo.RecordObject(pathFollower, "Straighten Path");
                Vector3 start = pathFollower.pathPoints[i].position;
                Vector3 end = pathFollower.pathPoints[i + 1].position;

                // Straighten by averaging the positions of the points in this segment
                pathFollower.pathPoints[i + 1].position = new Vector3((start.x + end.x) / 2, start.y, start.z); // Straight line along x
            }
        }
    }

    // Remove a point from the array
    private Transform[] RemovePoint(Transform[] points, int index)
    {
        var newPoints = new Transform[points.Length - 1];
        for (int i = 0, j = 0; i < points.Length; i++)
        {
            if (i != index)
            {
                newPoints[j++] = points[i];
            }
        }
        return newPoints;
    }

    // Check if a point is near a line segment
    private bool IsPointNearLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd, float threshold)
    {
        float distance = Vector3.Distance(point, GetClosestPointOnLine(point, lineStart, lineEnd));
        return distance < threshold;
    }

    // Get the closest point on a line segment to a given point
    private Vector3 GetClosestPointOnLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDirection = (lineEnd - lineStart).normalized;
        float lineLength = Vector3.Distance(lineStart, lineEnd);
        float projection = Mathf.Clamp(Vector3.Dot(point - lineStart, lineDirection), 0, lineLength);
        return lineStart + lineDirection * projection;
    }
}