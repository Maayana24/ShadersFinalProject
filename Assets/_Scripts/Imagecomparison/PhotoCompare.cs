using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotoCompare : MonoBehaviour
{
    [SerializeField] private CaptureImage captureImage;
    [SerializeField] private RawImage[] displayImages;
    [SerializeField] private ComputeShader compareShader;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Camera[] cameras;
    [SerializeField][Range(0.01f, 1f)] private float threshold = 0.20f;
    [SerializeField][Range(1f, 10f)] private float power = 2;
    [SerializeField] private bool isHighDifficulty = false;

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

    private float ComputeScore(Texture2D captured, Texture2D reference)
    {
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

        int groupsX = Mathf.CeilToInt(reference.width / 8f);
        int groupsY = Mathf.CeilToInt(reference.height / 8f);
        compareShader.Dispatch(_kernelIndex, groupsX, groupsY, 1);

        uint[] diff = new uint[1];
        uint[] pixels = new uint[1];
        _totalDiffBuffer.GetData(diff);
        _totalPixelsBuffer.GetData(pixels);

        capturedRT.Release();

        float avgDiff = (diff[0] / 10000f) / pixels[0] / 3f;
        float remapped = 1f - Mathf.Clamp01(avgDiff / threshold);
        return Mathf.Pow(remapped, power);
    }

    private IEnumerator CaptureAndCompare()
    {
        float totalScore = 0f;
        captureImage.SetSize(isHighDifficulty ? 512 : 64);

        for (int i = 0; i < cameras.Length; i++)
        {
            yield return StartCoroutine(captureImage.Capture(cameras[i]));

            Texture2D captured = captureImage.LastCapture;
            Texture2D reference = displayImages[i].texture as Texture2D;

            totalScore += ComputeScore(captured, reference);
        }

        float avgScore = totalScore / cameras.Length;
        resultText.text = $"Match: {avgScore * 100:F1}%";
    }

    void OnDestroy()
    {
        _totalDiffBuffer?.Release();
        _totalPixelsBuffer?.Release();
    }
}