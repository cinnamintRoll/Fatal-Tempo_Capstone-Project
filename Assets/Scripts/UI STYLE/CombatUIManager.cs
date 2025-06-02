using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI kanjiText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI actionText;

    [Header("Colors by Combo")]
    public Color grey = new Color32(0xc4, 0xc6, 0xc8, 0xFF);
    public Color green = new Color32(0x04, 0xf2, 0xa6, 0xFF);
    public Color blue = new Color32(0x04, 0x96, 0xff, 0xFF);
    public Color yellow = new Color32(0xf6, 0xee, 0x11, 0xFF);
    public Color redOrange = new Color32(0xfc, 0x69, 0x00, 0xFF);

    private int currentCombo = 0;
    private int totalPoints = 0;

    public void UpdateUI(string action)
    {
        totalPoints += GetPointsFromAction(action);
        currentCombo = Mathf.Clamp(currentCombo + GetComboDelta(action), 0, 999);

        // Update Kanji tier
        kanjiText.text = GetKanjiByCombo(currentCombo);

        // Update Action text
        actionText.text = action;

        // Update Points
        pointsText.text = totalPoints.ToString();

        // Change color based on combo
        Color comboColor = GetColorByCombo(currentCombo);
        ApplyColor(comboColor);
    }

    private string GetKanjiByCombo(int combo)
    {
        if (combo >= 50) return "極";
        if (combo >= 25) return "爆";
        if (combo >= 10) return "騒";
        return "";
    }

    private Color GetColorByCombo(int combo)
    {
        if (combo >= 50) return redOrange;
        if (combo >= 25) return yellow;
        if (combo >= 10) return blue;
        if (combo >= 5) return green;
        return grey;
    }

    private void ApplyColor(Color color)
    {
        kanjiText.color = color;
        pointsText.color = color;
        actionText.color = color;
    }

    private int GetPointsFromAction(string action)
    {
        switch (action)
        {
            case "Parried": return 50;
            case "Sliced": return 100;
            case "Blasted": return 150;
            case "Dodged": return 75;
            case "FATAL": return 500;
            case "Blunder": return -100;
            default: return 0;
        }
    }

    private int GetComboDelta(string action)
    {
        switch (action)
        {
            case "Blunder": return -currentCombo; // Reset combo
            case "FATAL": return 0;
            default: return 1;
        }
    }

    // For testing with keypresses (optional)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) UpdateUI("Parried");
        if (Input.GetKeyDown(KeyCode.Alpha2)) UpdateUI("Sliced");
        if (Input.GetKeyDown(KeyCode.Alpha3)) UpdateUI("Blasted");
        if (Input.GetKeyDown(KeyCode.Alpha4)) UpdateUI("Dodged");
        if (Input.GetKeyDown(KeyCode.Alpha5)) UpdateUI("FATAL");
        if (Input.GetKeyDown(KeyCode.Alpha0)) UpdateUI("Blunder");
    }
}
