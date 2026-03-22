using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CaptureImage : MonoBehaviour
{
    [SerializeField] private bool showDebug = true;
    [SerializeField] private RawImage targetImage;
    [SerializeField][Range(50, 1000)] private int size = 300;

    private Texture2D lastCapture;
    private RenderTexture renderTexture;

    public Texture2D LastCapture => lastCapture;

    private (int w, int h) GetCaptureDimensions()
    {
        float aspect = targetImage.rectTransform.rect.width / targetImage.rectTransform.rect.height;
        int w = size;
        int h = Mathf.RoundToInt(size / aspect);
        return (w, h);
    }

    public IEnumerator Capture(Camera camera)
    {
        yield return new WaitForEndOfFrame();

        var (w, h) = GetCaptureDimensions();

        if (renderTexture == null || renderTexture.width != w || renderTexture.height != h)
        {
            if (renderTexture != null) renderTexture.Release();
            renderTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        }

        RenderTexture previousTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        camera.Render();
        camera.targetTexture = previousTarget;

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = renderTexture;

        if (lastCapture != null) Destroy(lastCapture);
        lastCapture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        lastCapture.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        lastCapture.Apply();

        RenderTexture.active = previousActive;
    }

    void OnDestroy()
    {
        if (lastCapture != null) Destroy(lastCapture);
        if (renderTexture != null) renderTexture.Release();
    }
}