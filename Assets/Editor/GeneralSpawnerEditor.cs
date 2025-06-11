using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GeneralSpawner))]
public class GeneralSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty spawnablesProp = serializedObject.FindProperty("Spawnables");
        SerializedProperty spawnIndexProp = serializedObject.FindProperty("spawnIndex");

        GeneralSpawner spawner = (GeneralSpawner)target;

        // Draw spawnables list
        EditorGUILayout.PropertyField(spawnablesProp, true);

        // Show dynamic dropdown based on spawnables list
        if (spawner.Spawnables != null && spawner.Spawnables.Count > 0)
        {
            string[] names = new string[spawner.Spawnables.Count];
            for (int i = 0; i < names.Length; i++)
            {
                GameObject go = spawner.Spawnables[i].spawnable;
                names[i] = go != null ? go.name : "[Missing]";
            }

            spawnIndexProp.intValue = EditorGUILayout.Popup("Spawn Type", spawnIndexProp.intValue, names);
        }
        else
        {
            EditorGUILayout.LabelField("No spawnables assigned.");
        }

        // Draw other default fields except the ones we handled manually
        DrawPropertiesExcluding(serializedObject, "Spawnables", "spawnIndex");

        serializedObject.ApplyModifiedProperties();
    }

    // Custom scene view handling
    private void OnSceneGUI()
    {
        // Only run in Edit mode and in the Scene view
        if (Application.isPlaying || !SceneView.currentDrawingSceneView)
            return;

        GeneralSpawner spawner = (GeneralSpawner)target;

        // --- Handle spawnPoint ---
        if (spawner.spawnPoint != null)
        {
            EditorGUI.BeginChangeCheck();

            // Set color for spawnPoint handle
            Handles.color = Color.green;
            Vector3 newSpawnPosition = Handles.PositionHandle(spawner.spawnPoint.position, spawner.spawnPoint.rotation);

            // Draw a label for spawnPoint
            Handles.Label(spawner.spawnPoint.position + Vector3.up * 0.5f, "Spawn Point");

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner.spawnPoint, "Move Spawn Point");
                spawner.spawnPoint.position = newSpawnPosition;
            }
        }

        // --- Handle movepoint ---
        SerializedProperty movepointProp = serializedObject.FindProperty("movepoint");

        if (movepointProp != null && movepointProp.objectReferenceValue != null)
        {
            Transform movepointTransform = (Transform)movepointProp.objectReferenceValue;

            EditorGUI.BeginChangeCheck();

            // Set color for movepoint handle
            Handles.color = Color.blue;
            Vector3 newMovePosition = Handles.PositionHandle(movepointTransform.position, movepointTransform.rotation);

            // Draw a label for movepoint
            Handles.Label(movepointTransform.position + Vector3.up * 0.5f, "Move Point");

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(movepointTransform, "Move Move Point");
                movepointTransform.position = newMovePosition;
            }
        }

        // Reset handle color to default after drawing your custom handles
        Handles.color = Color.white;
    }
}