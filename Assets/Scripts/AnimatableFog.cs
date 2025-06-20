using UnityEngine;

[ExecuteAlways]
public class AnimatableFog : MonoBehaviour
{
    [Header("Animator Control")]
    public bool overrideFog = false;

    [Header("Fog Enabled")]
    public bool enableFog = true;

    [Header("Fog Mode (1 = Linear, 2 = Exp, 3 = Exp2)")]
    [Range(1, 3)]
    public int fogModeInt = 2;

    [Header("Fog Color")]
    public Color fogColor = Color.gray;

    [Header("Exponential Fog Settings")]
    public float fogDensity = 0.01f;

    [Header("Linear Fog Settings")]
    public float fogStartDistance = 0f;
    public float fogEndDistance = 300f;

    private void LateUpdate()
    {
        if (!overrideFog) return;

        RenderSettings.fog = enableFog;

        RenderSettings.fogMode = (FogMode)Mathf.Clamp(fogModeInt, 1, 3);
        RenderSettings.fogColor = fogColor;

        if (RenderSettings.fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }
        else
        {
            RenderSettings.fogDensity = fogDensity;
        }
    }
}
