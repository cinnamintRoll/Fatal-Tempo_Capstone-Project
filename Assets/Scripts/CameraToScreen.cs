using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class SpectatorRenderToScreen : MonoBehaviour
{
    [Header("Render Texture Format")]
    public RenderTextureFormat textureFormat = RenderTextureFormat.Default;

    private RenderTexture spectatorTexture;
    private RawImage rawImage;
    private Vector2Int lastResolution;
    private Camera cam;

    void Start()
    {
#if UNITY_ANDROID
        // Disable this component and camera on Android
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.enabled = false;
        }

        this.enabled = false;
        return;
#endif

        SetupSpectatorView();
    }

    void Update()
    {
        // Check if screen resolution changed
        if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
        {
            UpdateRenderTexture();
        }
    }

    void SetupSpectatorView()
    {
        lastResolution = new Vector2Int(Screen.width, Screen.height);

        spectatorTexture = new RenderTexture(Screen.width, Screen.height, 24, textureFormat);
        spectatorTexture.Create();

        cam = GetComponent<Camera>();
        cam.targetTexture = spectatorTexture;
        cam.targetDisplay = 0;
        cam.stereoTargetEye = StereoTargetEyeMask.None;

        CreateFullscreenCanvas();
    }

    void UpdateRenderTexture()
    {
        if (spectatorTexture != null)
        {
            spectatorTexture.Release();
        }

        lastResolution = new Vector2Int(Screen.width, Screen.height);
        spectatorTexture = new RenderTexture(Screen.width, Screen.height, 24, textureFormat);
        spectatorTexture.Create();

        cam.targetTexture = spectatorTexture;

        if (rawImage != null)
        {
            rawImage.texture = spectatorTexture;
        }
    }

    void CreateFullscreenCanvas()
    {
        GameObject canvasGO = new GameObject("SpectatorCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject rawImageGO = new GameObject("SpectatorImage", typeof(RawImage));
        rawImageGO.transform.SetParent(canvasGO.transform, false);
        rawImage = rawImageGO.GetComponent<RawImage>();
        rawImage.texture = spectatorTexture;
        rawImage.raycastTarget = false;

        RectTransform rect = rawImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
