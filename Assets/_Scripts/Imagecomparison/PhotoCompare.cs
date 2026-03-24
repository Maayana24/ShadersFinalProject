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
    [SerializeField][Range(0.01f, 1)] private float threshold = 0.20f;
    [SerializeField][Range(1, 10)] private float power = 2;
    [SerializeField] private bool isHighDifficulty = false;

    private ComputeBuffer _totalDiffBuffer;
    private ComputeBuffer _totalPixelsBuffer;
    private int _kernelIndex;

    void Start()
    {
        _kernelIndex = compareShader.FindKernel("Compare");
        _totalDiffBuffer = new ComputeBuffer(1, sizeof(int));
        _totalPixelsBuffer = new ComputeBuffer(1, sizeof(int));
    }

    [ContextMenu("Compare")]
    public void OnCheckButtonClicked()
    {
        StartCoroutine(CaptureAndCompare());
    }

    private float ComputeScore(Texture2D captured, Texture2D reference)
    {
        int compareSize = isHighDifficulty ? 512 : 64;

        RenderTexture capturedRT = new RenderTexture(compareSize, compareSize, 0, RenderTextureFormat.ARGB32);
        capturedRT.filterMode = FilterMode.Point;
        capturedRT.enableRandomWrite = true;
        capturedRT.Create();
        Graphics.Blit(captured, capturedRT);

        RenderTexture referenceRT = new RenderTexture(compareSize, compareSize, 0, RenderTextureFormat.ARGB32);
        referenceRT.filterMode = FilterMode.Point;
        referenceRT.enableRandomWrite = true;
        referenceRT.Create();
        Graphics.Blit(reference, referenceRT);

        _totalDiffBuffer.SetData(new int[] { 0 });
        _totalPixelsBuffer.SetData(new int[] { 0 });

        compareShader.SetTexture(_kernelIndex, "_Reference", referenceRT);
        compareShader.SetTexture(_kernelIndex, "_Captured", capturedRT);
        compareShader.SetBuffer(_kernelIndex, "_TotalDiff", _totalDiffBuffer);
        compareShader.SetBuffer(_kernelIndex, "_TotalPixels", _totalPixelsBuffer);
        compareShader.SetInt("_Width", compareSize);
        compareShader.SetInt("_Height", compareSize);

        float tolerance = isHighDifficulty ? 0.01f : 0.04f;
        compareShader.SetFloat("_Tolerance", tolerance);

        int groupsX = Mathf.CeilToInt(compareSize / 8);
        int groupsY = Mathf.CeilToInt(compareSize / 8);
        compareShader.Dispatch(_kernelIndex, groupsX, groupsY, 1);

        int[] diff = new int[1];
        int[] pixels = new int[1];
        _totalDiffBuffer.GetData(diff);
        _totalPixelsBuffer.GetData(pixels);

        capturedRT.Release();
        referenceRT.Release();

        float wrongPixelRatio = (float)diff[0] / pixels[0];
        return 1f - wrongPixelRatio;
    }

    private IEnumerator CaptureAndCompare()
    {
        float totalScore = 0;
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