using UnityEngine;
using System.Collections.Generic;

public class DemoModeManager : MonoBehaviour
{
    [SerializeField] List<GameObject> checkVisuals;
    [SerializeField] string DemoKey = "DemoMode";

    private void OnEnable()
    {
        bool demo = PlayerPrefs.GetInt(DemoKey, 0) == 1;
        UpdateVisuals(demo);
    }

    public void EnableDemoMode()
    {
        UpdateVisuals(true);
        PlayerPrefs.SetInt(DemoKey, 1);
        PlayerPrefs.Save();
        Debug.Log("Demo Mode Enabled");
    }

    public void DisableDemoMode()
    {
        UpdateVisuals(false);
        PlayerPrefs.SetInt(DemoKey, 0);
        PlayerPrefs.Save();
        Debug.Log("Demo Mode Disabled");
    }

    public void ToggleDemo()
    {
        bool demo = PlayerPrefs.GetInt(DemoKey, 0) == 1;
        if (demo)
        {
            DisableDemoMode();
        }
        else
        {
            EnableDemoMode();
        }
    }

    private void UpdateVisuals(bool active)
    {
        if (checkVisuals == null) return;

        foreach (var obj in checkVisuals)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
}
