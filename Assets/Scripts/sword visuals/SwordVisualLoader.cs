using System.Collections.Generic;
using UnityEngine;

public class SwordCosmeticHandler : MonoBehaviour
{
    [Tooltip("Where cosmetic visuals get spawned (i.e., the parent/holder of the sword visual)")]
    public Transform visualParent;

    [Tooltip("The transform representing where the hand grips the sword (on the loader)")]
    public Transform loaderHandleTransform;

    [Tooltip("Prefabs that contain a SwordCosmetic component")]
    public List<GameObject> cosmeticPrefabs;

    public int currentIndex = 0;
    private GameObject currentVisual;

    private const string PlayerPrefKey = "SelectedSwordCosmetic";

    private void Start()
    {
        currentIndex = PlayerPrefs.GetInt(PlayerPrefKey, 0);
        ApplyCosmetic(currentIndex);
    }

    public void ApplyCosmetic(int index)
    {
        Debug.Log("Trying to apply visual");

        if (index < 0 || index >= cosmeticPrefabs.Count) return;

        if (currentVisual != null)
        {
            Destroy(currentVisual);
            currentVisual = null;
        }


        GameObject prefab = cosmeticPrefabs[index];
        if (prefab == null)
        {
            Debug.Log("Didn't find the object");
            return;
        }

        Debug.Log("Prefab found");

        currentVisual = Instantiate(prefab, visualParent);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
        currentVisual.transform.localScale = Vector3.one;

        SwordCosmetic cosmetic = currentVisual.GetComponent<SwordCosmetic>();

        if (cosmetic != null && cosmetic.handleTransform != null && loaderHandleTransform != null)
        {
            Transform handle = cosmetic.handleTransform;

            // Step 1: Match handle's world pose to the loader's
            // This gives us the handle delta relative to the visual root
            Matrix4x4 handleToWorld = loaderHandleTransform.localToWorldMatrix;
            Matrix4x4 visualToHandle = handle.worldToLocalMatrix * currentVisual.transform.localToWorldMatrix;
            Matrix4x4 newVisualMatrix = handleToWorld * visualToHandle;

            // Step 2: Apply new matrix to visual
            currentVisual.transform.position = newVisualMatrix.GetColumn(3);
            currentVisual.transform.rotation = newVisualMatrix.rotation;

            // Step 3: Reparent to visualParent while keeping world alignment
            currentVisual.transform.SetParent(visualParent, true);
        }




        PlayerPrefs.SetInt(PlayerPrefKey, index);
        PlayerPrefs.Save();

        var slicer = GetComponent<SliceObject>();
        if (slicer != null)
        {
            slicer.bladeStart = currentVisual.transform.Find("BladeStart");
            slicer.bladeEnd = currentVisual.transform.Find("BladeEnd");
        }
    }
#if UNITY_EDITOR
    private GameObject previewInstance;

    [ContextMenu("Preview Cosmetic In Scene")]
    private void PreviewCosmeticInScene()
    {
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
            previewInstance = null;
        }

        if (currentIndex < 0 || currentIndex >= cosmeticPrefabs.Count)
        {
            Debug.LogWarning("Invalid index.");
            return;
        }

        GameObject prefab = cosmeticPrefabs[currentIndex];
        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null.");
            return;
        }

        previewInstance = Instantiate(prefab);
        previewInstance.name = "SwordPreview";

        SwordCosmetic cosmetic = previewInstance.GetComponent<SwordCosmetic>();

        if (cosmetic != null && cosmetic.handleTransform != null && loaderHandleTransform != null)
        {
            Transform handle = cosmetic.handleTransform;

            // Step 1: Match handle's world pose to the loader's
            Matrix4x4 handleToWorld = loaderHandleTransform.localToWorldMatrix;
            Matrix4x4 visualToHandle = handle.worldToLocalMatrix * previewInstance.transform.localToWorldMatrix;
            Matrix4x4 newVisualMatrix = handleToWorld * visualToHandle;

            previewInstance.transform.position = newVisualMatrix.GetColumn(3);
            previewInstance.transform.rotation = newVisualMatrix.rotation;
            previewInstance.transform.SetParent(visualParent, true);
            previewInstance.transform.localScale = Vector3.one;
        }
    }

    [ContextMenu("Clear Preview")]
    private void ClearPreview()
    {
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
            previewInstance = null;
        }
    }
#endif


#if UNITY_EDITOR
    [ContextMenu("Update Visual")]
    private void UpdateVisual()
    {
        PlayerPrefs.SetInt(PlayerPrefKey, currentIndex);

        if (Application.isPlaying)
        {
            ApplyCosmetic(currentIndex);
        }
        else
        {
            Debug.LogWarning("Update Visual only works at runtime. Enter Play Mode to see the visual change.");
        }
    }
#endif
}
