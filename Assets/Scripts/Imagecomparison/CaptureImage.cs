using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CaptureImage : MonoBehaviour
{
    [SerializeField] private bool showDebug = true;
    [SerializeField] private RawImage targetImage;
    [SerializeField][Range(50, 1000)] private int size = 300;

    private Texture2D lastCapture;
    public Texture2D LastCapture => lastCapture;

    private Rect GetCaptureRect()
    {
        float aspect = targetImage.rectTransform.rect.width / targetImage.rectTransform.rect.height;
        int w = size;
        int h = Mathf.RoundToInt(size / aspect);
        int x = (Screen.width - w) / 2;
        int y = (Screen.height - h) / 2;
        return new Rect(x, y, w, h);
    }

    public IEnumerator Capture()
    {
        yield return new WaitForEndOfFrame();

        Rect r = GetCaptureRect();
        int x = Mathf.Clamp((int)r.x, 0, Screen.width);
        int y = Mathf.Clamp((int)r.y, 0, Screen.height);
        int w = Mathf.Clamp((int)r.width, 1, Screen.width - x);
        int h = Mathf.Clamp((int)r.height, 1, Screen.height - y);

        lastCapture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        lastCapture.ReadPixels(new Rect(x, y, w, h), 0, 0);
        lastCapture.Apply();
    }

    void OnGUI()
    {
        if (!showDebug) return;

        Rect r = GetCaptureRect();
        float guiY = Screen.height - r.y - r.height;
        float b = 3f;

        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(r.x, guiY, r.width, b), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x, guiY + r.height, r.width, b), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x, guiY, b, r.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x + r.width, guiY, b, r.height + b), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }

    void OnDestroy()
    {
        if (lastCapture != null) Destroy(lastCapture);
    }
}