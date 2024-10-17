using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerHealth))]
public class PlayerHealthEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        PlayerHealth playerHealth = (PlayerHealth)target;

        // Add a button for taking damage
        if (GUILayout.Button("Take Damage (Debug)"))
        {
            playerHealth.DebugTakeDamage();
        }

        // Add a button for killing an enemy
        if (GUILayout.Button("Kill Enemy (Debug)"))
        {
            playerHealth.DebugKillEnemy();
        }
    }
}
