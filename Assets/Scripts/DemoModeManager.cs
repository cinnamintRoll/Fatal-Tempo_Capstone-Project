using UnityEngine;
using UnityEngine.UIElements;

public class DemoModeManager : MonoBehaviour
{
    [SerializeField] GameObject checkVisual;
    [SerializeField] string DemoKey = "DemoMode";

    private void OnEnable()
    {
        bool demo = PlayerPrefs.GetInt(DemoKey, 0) == 1;
        if (checkVisual != null) { 
            checkVisual.SetActive(demo);
        }
    }

    public void EnableDemoMode()
    {
        if (checkVisual != null)
        {
            checkVisual.SetActive(true);
        }
        PlayerPrefs.SetInt(DemoKey, 1);
        PlayerPrefs.Save();
        Debug.Log("Demo Mode Enabled");
       
    }

    public void DisableDemoMode()
    {
        if (checkVisual != null)
        {
            checkVisual.SetActive(false);
        }
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
}
