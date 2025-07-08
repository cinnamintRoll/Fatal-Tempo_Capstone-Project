using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GunSkinSet
{
    public List<GameObject> skinParts;
}

public class GunSkinHandler : MonoBehaviour
{
    [Tooltip("List of available skin sets")]
    public List<GunSkinSet> skinSets = new List<GunSkinSet>();

    [Tooltip("Default skin index to use on first launch")]
    public int defaultSkinIndex = 0;

    [Tooltip("Currently selected skin index")]
    public int selectedSkinIndex = 0;

    private const string SkinPrefKey = "SelectedGunSkinIndex";

    private int lastPreviewedIndex = -1;

    void Awake()
    {
        int savedIndex = PlayerPrefs.GetInt(SkinPrefKey, defaultSkinIndex);
        SetSkin(savedIndex);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && selectedSkinIndex != lastPreviewedIndex)
        {
            SetSkin(selectedSkinIndex);
        }
    }
#endif

    public void SetSkin(int index)
    {
        if (skinSets.Count == 0) return;
        index = Mathf.Clamp(index, 0, skinSets.Count - 1);

        // Collect all unique objects across all sets
        HashSet<GameObject> allObjects = new HashSet<GameObject>();
        foreach (var set in skinSets)
        {
            foreach (var obj in set.skinParts)
            {
                if (obj != null)
                    allObjects.Add(obj);
            }
        }

        // Enable objects in selected set
        HashSet<GameObject> activeObjects = new HashSet<GameObject>();
        foreach (var obj in skinSets[index].skinParts)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                activeObjects.Add(obj);
            }
        }

        // Disable all other objects that aren't part of the active set
        foreach (var obj in allObjects)
        {
            if (!activeObjects.Contains(obj))
            {
                obj.SetActive(false);
            }
        }

        selectedSkinIndex = index;
        lastPreviewedIndex = index;
        PlayerPrefs.SetInt(SkinPrefKey, index);
        PlayerPrefs.Save();
    }

    public void NextSkin()
    {
        if (skinSets.Count == 0) return;
        int nextIndex = (selectedSkinIndex + 1) % skinSets.Count;
        SetSkin(nextIndex);
    }

    public void PreviousSkin()
    {
        if (skinSets.Count == 0) return;
        int prevIndex = (selectedSkinIndex - 1 + skinSets.Count) % skinSets.Count;
        SetSkin(prevIndex);
    }

    public int GetCurrentSkinIndex()
    {
        return selectedSkinIndex;
    }

    public int GetSkinCount()
    {
        return skinSets.Count;
    }
}
