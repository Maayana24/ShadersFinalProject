using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotoCompare : MonoBehaviour
{
    [SerializeField] private CaptureImage captureImage;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private ComputeShader compareShader;
    [SerializeField] private TextMeshProUGUI resultText;

    [SerializeField][Range(0.01f, 1f)] private float threshold = 0.20f;
    [SerializeField][Range(1f, 10f)] private float power = 2;

    private ComputeBuffer _totalDiffBuffer;
    private ComputeBuffer _totalPixelsBuffer;
    private int _kernelIndex;

    void Start()
    {
        _kernelIndex = compareShader.FindKernel("Compare");
        _totalDiffBuffer = new ComputeBuffer(1, sizeof(uint));
        _totalPixelsBuffer = new ComputeBuffer(1, sizeof(uint));
    }

    [ContextMenu("Compare")]
    public void OnCheckButtonClicked()
    {
        StartCoroutine(CaptureAndCompare());
    }

    private IEnumerator CaptureAndCompare()
    {
        yield return StartCoroutine(captureImage.Capture());
        Texture2D captured = captureImage.LastCapture;

        Texture2D reference = displayImage.texture as Texture2D;

        RenderTexture capturedRT = new RenderTexture(reference.width, reference.height, 0, RenderTextureFormat.ARGB32);
        capturedRT.enableRandomWrite = true;
        capturedRT.Create();
        Graphics.Blit(captured, capturedRT);

        _totalDiffBuffer.SetData(new uint[] { 0 });
        _totalPixelsBuffer.SetData(new uint[] { 0 });

        compareShader.SetTexture(_kernelIndex, "_Reference", reference);
        compareShader.SetTexture(_kernelIndex, "_Captured", capturedRT);
        compareShader.SetBuffer(_kernelIndex, "_TotalDiff", _totalDiffBuffer);
        compareShader.SetBuffer(_kernelIndex, "_TotalPixels", _totalPixelsBuffer);
        compareShader.SetInt("_Width", reference.width);
        compareShader.SetInt("_Height", reference.height);

        int groupsX = Mathf.CeilToInt(reference.width / 8);
        int groupsY = Mathf.CeilToInt(reference.height / 8);
        compareShader.Dispatch(_kernelIndex, groupsX, groupsY, 1);

        uint[] diff = new uint[1];
        uint[] pixels = new uint[1];
        _totalDiffBuffer.GetData(diff);
        _totalPixelsBuffer.GetData(pixels);

        float avgDiff = (diff[0] / 10000f) / pixels[0] / 3f;
        float remapped = 1f - Mathf.Clamp01(avgDiff / threshold);
        float score = Mathf.Pow(remapped, power);

        string msg = $"Match: {score * 100:F1}%";
        resultText.text = msg;
        capturedRT.Release();
    }

    void OnDestroy()
    {
        _totalDiffBuffer?.Release();
        _totalPixelsBuffer?.Release();
    }
}