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

    void Start()
    {
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
        // Initial resolution
        lastResolution = new Vector2Int(Screen.width, Screen.height);

        // Create RenderTexture
        spectatorTexture = new RenderTexture(Screen.width, Screen.height, 24, textureFormat);
        spectatorTexture.Create();

        // Assign to this camera
        Camera cam = GetComponent<Camera>();
        cam.targetTexture = spectatorTexture;
        cam.targetDisplay = 0; // Display 1
        cam.stereoTargetEye = StereoTargetEyeMask.None;

        // Setup full-screen UI with RawImage
        CreateFullscreenCanvas();
    }

    void UpdateRenderTexture()
    {
        // Clean up old texture
        if (spectatorTexture != null)
        {
            spectatorTexture.Release();
        }

        // Create new one
        lastResolution = new Vector2Int(Screen.width, Screen.height);
        spectatorTexture = new RenderTexture(Screen.width, Screen.height, 24, textureFormat);
        spectatorTexture.Create();

        // Assign to camera and RawImage
        Camera cam = GetComponent<Camera>();
        cam.targetTexture = spectatorTexture;

        if (rawImage != null)
        {
            rawImage.texture = spectatorTexture;
        }
    }

    void CreateFullscreenCanvas()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("SpectatorCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Optional: Scale for reference resolution
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create RawImage
        GameObject rawImageGO = new GameObject("SpectatorImage", typeof(RawImage));
        rawImageGO.transform.SetParent(canvasGO.transform, false);
        rawImage = rawImageGO.GetComponent<RawImage>();
        rawImage.texture = spectatorTexture;
        rawImage.raycastTarget = false;

        // Stretch full screen
        RectTransform rect = rawImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
