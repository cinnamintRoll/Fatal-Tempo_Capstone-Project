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

    void Awake()
    {
        // Disable all skin parts (once)
       
        // Load saved skin index
        int savedIndex = PlayerPrefs.GetInt(SkinPrefKey, defaultSkinIndex);
        selectedSkinIndex = Mathf.Clamp(savedIndex, 0, skinSets.Count - 1);

        // Only enable selected
        ApplySkin(selectedSkinIndex);
    }

    private void ApplySkin(int index)
    {
        if (skinSets.Count == 0 || index < 0 || index >= skinSets.Count)
            return;
        foreach (var set in skinSets)
        {
            foreach (var part in set.skinParts)
            {
                if (part != null)
                    part.SetActive(false);
            }
        }

        // Enable only the selected skin parts
        foreach (var part in skinSets[index].skinParts)
        {
            if (part != null)
                part.SetActive(true);
        }

        selectedSkinIndex = index;
        PlayerPrefs.SetInt(SkinPrefKey, index);
        PlayerPrefs.Save();
    }

    public void SetSkin(int index)
    {
        ApplySkin(index);
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
