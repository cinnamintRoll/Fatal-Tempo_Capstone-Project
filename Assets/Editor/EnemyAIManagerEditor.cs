using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyAIManager))]
[CanEditMultipleObjects]
public class EnemyAIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Manual Sync Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Sync From EnemyAI"))
        {
            foreach (Object obj in targets)
            {
                EnemyAIManager manager = (EnemyAIManager)obj;
                manager.SyncFromEnemyAI();
            }
        }

        if (GUILayout.Button("Apply Changes to Enemy"))
        {
            foreach (Object obj in targets)
            {
                EnemyAIManager manager = (EnemyAIManager)obj;
                manager.ApplyChangesToEnemy();
            }
        }
    }
}
