#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(WeaponPickup))]
public class WeaponPickupEditor : Editor
{
    SerializedProperty pickupModeProp;
    SerializedProperty singleWeaponTypeProp;
    SerializedProperty dualLeftWeaponTypeProp;
    SerializedProperty dualRightWeaponTypeProp;
    SerializedProperty weaponVisualsProp;
    SerializedProperty duplicateSpawnProp;

    void OnEnable()
    {
        pickupModeProp = serializedObject.FindProperty("pickupMode");
        singleWeaponTypeProp = serializedObject.FindProperty("singleWeaponType");
        dualLeftWeaponTypeProp = serializedObject.FindProperty("dualLeftWeaponType");
        dualRightWeaponTypeProp = serializedObject.FindProperty("dualRightWeaponType");
        weaponVisualsProp = serializedObject.FindProperty("weaponVisuals");
        duplicateSpawnProp = serializedObject.FindProperty("DuplicateSpawn");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(weaponVisualsProp, true);

        EditorGUILayout.PropertyField(pickupModeProp);
        var pickupMode = (WeaponPickup.PickupMode)pickupModeProp.enumValueIndex;

        if (pickupMode == WeaponPickup.PickupMode.Single)
        {
            EditorGUILayout.PropertyField(singleWeaponTypeProp);
        }
        else
        {
            EditorGUILayout.PropertyField(dualLeftWeaponTypeProp);
            EditorGUILayout.PropertyField(dualRightWeaponTypeProp);
        }

        EditorGUILayout.PropertyField(duplicateSpawnProp);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
