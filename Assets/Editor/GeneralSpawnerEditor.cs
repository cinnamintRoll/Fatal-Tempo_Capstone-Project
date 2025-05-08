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
                GameObject go = spawner.Spawnables[i];
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

        if (spawner.spawnPoint != null)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(spawner.spawnPoint.position, spawner.spawnPoint.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spawner.spawnPoint, "Move Spawn Point");
                spawner.spawnPoint.position = newPosition;
            }

            // Preview the selected spawnable
            if (spawner.Spawnables.Count > 0 && spawner.Spawnables[spawner.spawnIndex] != null)
            {
                GameObject previewObject = spawner.Spawnables[spawner.spawnIndex];
                if (previewObject != null)
                {
                    Renderer renderer = previewObject.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        Vector3 objectWorldPosition = spawner.spawnPoint.position;
                        objectWorldPosition.y = renderer.bounds.center.y;

                        Handles.color = new Color(1f, 0f, 0f, 0.5f);
                        Handles.DrawWireCube(objectWorldPosition, renderer.bounds.size);
                    }
                }
            }
        }
    }

}
