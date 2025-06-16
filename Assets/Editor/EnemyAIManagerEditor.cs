using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyAIManager))]
public class EnemyAIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyAIManager manager = (EnemyAIManager)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Manual Sync Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Sync From EnemyAI"))
        {
            manager.SyncFromEnemyAI();
        }

        if (GUILayout.Button("Apply Changes to Enemy"))
        {
            manager.ApplyChangesToEnemy();
        }
    }
}
