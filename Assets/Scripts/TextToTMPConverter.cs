#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class TextToTMPConverter : MonoBehaviour
{
    [MenuItem("Tools/Convert All Legacy Texts to TMP in Scene")]
    public static void ConvertTextToTMP()
    {
        Text[] legacyTexts = FindObjectsOfType<Text>(true);
        int converted = 0;

        foreach (Text legacy in legacyTexts)
        {
            GameObject go = legacy.gameObject;

            // Backup the original text
            string originalText = legacy.text;
            Font font = legacy.font;
            Color color = legacy.color;
            int fontSize = legacy.fontSize;
            TextAnchor alignment = legacy.alignment;
            FontStyle fontStyle = legacy.fontStyle;
            bool raycastTarget = legacy.raycastTarget;
            HorizontalWrapMode hWrap = legacy.horizontalOverflow;
            VerticalWrapMode vWrap = legacy.verticalOverflow;

            // Remove legacy Text
            DestroyImmediate(legacy);

            // Add TMP component
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();

            // Apply previous values
            tmp.text = originalText;
            tmp.color = color;
            tmp.fontSize = fontSize;
            tmp.alignment = (TextAlignmentOptions)(int)alignment;
            tmp.fontStyle = (FontStyles)(int)fontStyle;
            tmp.raycastTarget = raycastTarget;
            tmp.enableWordWrapping = (hWrap != HorizontalWrapMode.Overflow);
            tmp.overflowMode = (vWrap == VerticalWrapMode.Overflow) ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;

            converted++;
        }

        Debug.Log($"Converted {converted} legacy Text components to TextMeshProUGUI.");
    }
}
#endif