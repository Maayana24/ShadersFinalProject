using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotoCompare : MonoBehaviour
{
    [SerializeField] private ComputeShader compareShader;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private SheepWoolManager sheepWoolManager;
    [SerializeField][Range(0f, 1f)] private float displacementTolerance = 0.1f;
    [SerializeField][Range(0f, 1f)] private float colorTolerance        = 0.15f;
    [SerializeField][Range(0f, 1f)] private float displacementWeight    = 0.7f;
    [SerializeField][Range(0f, 1f)] private float colorWeight           = 0.3f;
    [SerializeField] private float wrongThreshold = 0.7f; //fails if color or placement is wrong
    [SerializeField] private bool isHighDifficulty = false;
    [SerializeField][Range(0.1f, 1f)] private float hardDifficultyScale = 0.5f;

    private Texture2D currentWoolReference;
    private Texture2D woolSnapshot;

    private ComputeBuffer totalDiffBuffer;
    private ComputeBuffer totalPixelsBuffer;
    private int _kernelIndex;

    public static event Action<float> OnScore;

    void Awake()
    {
        UIManager.OnDifficultyChange += ChangeDifficulty;
        UIManager.OnImageChanged += OnImageChanged;
    }

    void Start()
    {
        _kernelIndex = compareShader.FindKernel("Compare");
        totalDiffBuffer = new ComputeBuffer(1, sizeof(int));
        totalPixelsBuffer = new ComputeBuffer(1, sizeof(int));
        OnImageChanged(UIManager.CurrentReference);
    }

    private void OnImageChanged(ImageReference reference)
    {
        currentWoolReference = reference.WoolTexture;
    }

    [ContextMenu("Compare")]
    public void OnCheckButtonClicked()
    {
        StartCoroutine(CaptureAndCompare());
    }

    private void ChangeDifficulty(bool value)
    {
        isHighDifficulty = value;
    }

    private float ComputeScore(Texture2D captured, Texture2D reference)
    {
        int compareSize = isHighDifficulty ? 512 : 64;
        float difficultyScale = isHighDifficulty ? hardDifficultyScale : 1f;

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

        totalDiffBuffer.SetData(new int[] { 0 });
        totalPixelsBuffer.SetData(new int[] { 0 });

        compareShader.SetTexture(_kernelIndex, "_Reference", referenceRT);
        compareShader.SetTexture(_kernelIndex, "_Captured", capturedRT);
        compareShader.SetBuffer(_kernelIndex, "_TotalDiff", totalDiffBuffer);
        compareShader.SetBuffer(_kernelIndex, "_TotalPixels", totalPixelsBuffer);
        compareShader.SetInt("_Width", compareSize);
        compareShader.SetInt("_Height", compareSize);

        compareShader.SetFloat("_AlphaTolerance", displacementTolerance * difficultyScale);
        compareShader.SetFloat("_ColorTolerance", colorTolerance * difficultyScale);
        compareShader.SetFloat("_AlphaWeight", displacementWeight);
        compareShader.SetFloat("_ColorWeight", colorWeight);
        compareShader.SetFloat("_WrongThreshold", wrongThreshold);

        int groupsX = Mathf.CeilToInt(compareSize / 8);
        int groupsY = Mathf.CeilToInt(compareSize / 8);
        compareShader.Dispatch(_kernelIndex, groupsX, groupsY, 1);

        int[] diff = new int[1];
        int[] pixels = new int[1];
        totalDiffBuffer.GetData(diff);
        totalPixelsBuffer.GetData(pixels);

        capturedRT.Release();
        referenceRT.Release();

        float wrongPixelRatio = (float)diff[0] / pixels[0];
        OnScore?.Invoke(1f - wrongPixelRatio);
        return 1f - wrongPixelRatio;
    }

    private IEnumerator CaptureAndCompare()
    {
        if (sheepWoolManager == null || currentWoolReference == null)
        {
            resultText.text = "Missing wool reference";
            yield break;
        }

        yield return new WaitForEndOfFrame();

        RenderTexture woolRT = sheepWoolManager.WoolTexture;
        if (woolSnapshot != null) Destroy(woolSnapshot);
        woolSnapshot = new Texture2D(woolRT.width, woolRT.height, TextureFormat.RGBA32, false);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = woolRT;
        woolSnapshot.ReadPixels(new Rect(0, 0, woolRT.width, woolRT.height), 0, 0);
        woolSnapshot.Apply();
        RenderTexture.active = prev;

        float score = ComputeScore(woolSnapshot, currentWoolReference);
        resultText.text = $"Match: {score * 100:F1}%";
    }

    void OnDestroy()
    {
        totalDiffBuffer?.Release();
        totalPixelsBuffer?.Release();
        if (woolSnapshot != null) Destroy(woolSnapshot);
        UIManager.OnDifficultyChange -= ChangeDifficulty;
        UIManager.OnImageChanged -= OnImageChanged;
    }
}
