using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
    private GameObject previewInstance;
    private int lastPreviewedIndex = -1;
#endif

    private void Start()
    {
        currentIndex = PlayerPrefs.GetInt(PlayerPrefKey, 0);
        ApplyCosmetic(currentIndex);
    }

    public void ApplyCosmetic(int index)
    {
        if (index < 0 || index >= cosmeticPrefabs.Count)
            return;

        if (currentVisual != null)
        {
            Destroy(currentVisual);
            currentVisual = null;
        }

        currentVisual = CreateCosmeticInstance(index, isPreview: false);

        PlayerPrefs.SetInt(PlayerPrefKey, index);
        PlayerPrefs.Save();

        var slicer = GetComponent<SliceObject>();
        if (slicer != null && currentVisual != null)
        {
            slicer.bladeStart = currentVisual.transform.Find("BladeStart");
            slicer.bladeEnd = currentVisual.transform.Find("BladeEnd");
        }
    }

    private GameObject CreateCosmeticInstance(int index, bool isPreview)
    {
        if (index < 0 || index >= cosmeticPrefabs.Count)
            return null;

        var prefab = cosmeticPrefabs[index];
        if (prefab == null || loaderHandleTransform == null)
            return null;

        GameObject instance = Instantiate(prefab);
        instance.name = isPreview ? "SwordPreview" : prefab.name;
        instance.transform.localScale = Vector3.one;

#if UNITY_EDITOR
        if (isPreview)
        {
            instance.hideFlags = HideFlags.DontSaveInEditor;
        }
#endif

        SwordCosmetic cosmetic = instance.GetComponent<SwordCosmetic>();
        if (cosmetic != null && cosmetic.handleTransform != null)
        {
            Transform handle = cosmetic.handleTransform;

            Matrix4x4 handleToWorld = loaderHandleTransform.localToWorldMatrix;
            Matrix4x4 visualToHandle = handle.worldToLocalMatrix * instance.transform.localToWorldMatrix;
            Matrix4x4 newVisualMatrix = handleToWorld * visualToHandle;

            instance.transform.position = newVisualMatrix.GetColumn(3);
            instance.transform.rotation = newVisualMatrix.rotation;
            instance.transform.SetParent(visualParent, true);
            instance.transform.localScale = Vector3.one;

        }

        return instance;
    }
    public void NextCosmetic()
    {
        int newIndex = (currentIndex + 1) % cosmeticPrefabs.Count;
        currentIndex = newIndex;
        ApplyCosmetic(currentIndex);
    }

    public void PreviousCosmetic()
    {
        int newIndex = (currentIndex - 1 + cosmeticPrefabs.Count) % cosmeticPrefabs.Count;
        currentIndex = newIndex;
        ApplyCosmetic(currentIndex);
    }

    public void SetCosmetic(int index)
    {
        if (index >= 0 && index < cosmeticPrefabs.Count)
        {
            currentIndex = index;
            ApplyCosmetic(currentIndex);
        }
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        if (Application.isPlaying || EditorUtility.IsPersistent(gameObject))
            return;

        int savedIndex = PlayerPrefs.GetInt(PlayerPrefKey, 0);

        // Keep currentIndex in sync with PlayerPrefs and track changes
        if (savedIndex != lastPreviewedIndex)
        {
            currentIndex = savedIndex;
            lastPreviewedIndex = savedIndex;

            ClearPreview();
            EditorApplication.delayCall += () =>
            {
                if (this == null || Application.isPlaying || EditorUtility.IsPersistent(gameObject))
                    return;

                previewInstance = CreateCosmeticInstance(currentIndex, isPreview: true);
            };
        }
        else if (currentIndex != lastPreviewedIndex)
        {
            //PlayerPrefs.SetInt(PlayerPrefKey, currentIndex);
            //PlayerPrefs.Save();
            lastPreviewedIndex = currentIndex;

            ClearPreview();
            EditorApplication.delayCall += () =>
            {
                if (this == null || Application.isPlaying || EditorUtility.IsPersistent(gameObject))
                    return;

                previewInstance = CreateCosmeticInstance(currentIndex, isPreview: true);
            };
        }
    }




    [ContextMenu("Preview Cosmetic In Scene")]
    private void PreviewCosmeticInScene()
    {
        ClearPreview();
        previewInstance = CreateCosmeticInstance(currentIndex, isPreview: true);
    }

    private void ClearPreview()
    {
        EditorApplication.delayCall += () =>
        {
            var existing = GameObject.Find("SwordPreview");

            if (previewInstance != null)
            {
                Object.DestroyImmediate(previewInstance);
                previewInstance = null;
            }
        };
    }

    [ContextMenu("Update Visual")]
    private void UpdateVisual()
    {
        if (currentVisual != null)
        {
            DestroyImmediate(currentVisual);
            currentVisual = null;
        }

        ClearPreview();

        PlayerPrefs.SetInt(PlayerPrefKey, currentIndex);
        ApplyCosmetic(currentIndex);
    }

    private void OnDisable()
    {
        ClearPreview();
    }
#endif
}
