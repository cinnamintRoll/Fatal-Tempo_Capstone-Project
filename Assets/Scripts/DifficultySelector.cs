using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    [Header("UI Images for Buttons")]
    public Image easyModeImage;
    public Image normalModeImage;

    [Header("Color Settings")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.white;

    private const string EasyModeKey = "EasyMode"; // 1 = Easy, 0 = Normal

    void Start()
    {
        UpdateModeButtonHighlights();
    }

    /// <summary>
    /// Call this from the Easy button's OnClick and pass true.
    /// Call this from the Normal button's OnClick and pass false.
    /// </summary>
    public void SetEasyMode(bool isEasy)
    {
        PlayerPrefs.SetInt(EasyModeKey, isEasy ? 1 : 0);
        PlayerPrefs.Save();
        UpdateModeButtonHighlights();
    }

    /// <summary>
    /// Updates the UI visuals based on the current EasyMode PlayerPrefs value.
    /// </summary>
    private void UpdateModeButtonHighlights()
    {
        bool isEasy = PlayerPrefs.GetInt(EasyModeKey, 0) == 1;

        if (easyModeImage != null)
            easyModeImage.color = isEasy ? activeColor : inactiveColor;

        if (normalModeImage != null)
            normalModeImage.color = isEasy ? inactiveColor : activeColor;
    }
}
