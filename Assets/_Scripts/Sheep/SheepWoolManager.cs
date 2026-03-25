using UnityEngine;

public class SheepWoolManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer woolRenderer;
    [SerializeField] private MeshFilter woolMeshFilter;
    [SerializeField] private MeshCollider woolMeshCollider;

    [Header("Texture")]
    [SerializeField] private int textureSize = 512;
    [SerializeField] private Color baseWoolColor = Color.white;
    [SerializeField] private float noiseScale = 8f;
    [SerializeField] private float noiseMin = 0.4f;
    [SerializeField] private float noiseMax = 1f;

    [Header("Displacement")]
    [SerializeField] private float maxDisplacement = 0.3f;

    [Header("Tessellation")]
    [SerializeField, Range(1, 16)] private float tessMin = 1f;
    [SerializeField, Range(1, 64)] private float tessMax = 8f;
    [SerializeField] private float tessDistMin = 2f;
    [SerializeField] private float tessDistMax = 15f;

    public float MaxDisplacement => maxDisplacement;
    public RenderTexture WoolTexture => woolRT;

    private RenderTexture woolRT;
    private Material woolMaterial;
    private float worldToUVScale;
    private Mesh inflatedColliderMesh;

    private void OnValidate()
    {
        if (woolRenderer == null) woolRenderer = GetComponentInChildren<Renderer>();
        if (woolMeshFilter == null) woolMeshFilter = GetComponentInChildren<MeshFilter>();
        if (woolMeshCollider == null) woolMeshCollider = GetComponentInChildren<MeshCollider>();
        if (woolRenderer != null) PushToMaterial(woolRenderer.sharedMaterial);
    }

    private void Awake()
    {
        // Push tessellation config to sharedMaterial before instantiating via woolRenderer.material.
        PushToMaterial(woolRenderer.sharedMaterial);

        woolRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        woolRT.filterMode = FilterMode.Bilinear;
        woolRT.Create();
        InitializeTexture();

        woolMaterial = woolRenderer.material;
        woolMaterial.SetTexture("_WoolTex", woolRT);

        if (woolMeshFilter != null)
        {
            worldToUVScale = 1f / Mathf.Max(woolMeshFilter.sharedMesh.bounds.size.x, woolMeshFilter.sharedMesh.bounds.size.z);
            if (woolMeshCollider != null)
                woolMeshCollider.sharedMesh = BuildInflatedMesh(woolMeshFilter.sharedMesh);
        }
        else
            worldToUVScale = 1f;
    }

    // Seeds the wool texture with Perlin noise on the CPU — no shader dependency.
    private void InitializeTexture()
    {
        float offsetX = Random.value * 100f;
        float offsetY = Random.value * 100f;

        Color[] pixels = new Color[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float v = y / (float)textureSize;
                float nx = (x / (float)textureSize + offsetX) * noiseScale;
                float ny = (v + offsetY) * noiseScale;
                float growth = Mathf.Lerp(noiseMin, noiseMax, Mathf.PerlinNoise(nx, ny));
                float edge = Mathf.InverseLerp(1f, 0.9f, v);
                pixels[y * textureSize + x] = new Color(baseWoolColor.r, baseWoolColor.g, baseWoolColor.b, growth * edge);
            }
        }

        Texture2D noiseTex = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        noiseTex.SetPixels(pixels);
        noiseTex.Apply();
        Graphics.Blit(noiseTex, woolRT);
        Destroy(noiseTex);
    }

    private Mesh BuildInflatedMesh(Mesh source)
    {
        inflatedColliderMesh = Instantiate(source);
        Vector3[] verts = inflatedColliderMesh.vertices;
        Vector3[] normals = inflatedColliderMesh.normals;
        for (int i = 0; i < verts.Length; i++)
            verts[i] += normals[i] * maxDisplacement;
        inflatedColliderMesh.vertices = verts;
        inflatedColliderMesh.RecalculateBounds();
        return inflatedColliderMesh;
    }

    private void PushToMaterial(Material mat)
    {
        if (mat == null) return;
        mat.SetFloat("_MaxDisplacement", maxDisplacement);
        mat.SetFloat("_TessMin", tessMin);
        mat.SetFloat("_TessMax", tessMax);
        mat.SetFloat("_TessDistMin", tessDistMin);
        mat.SetFloat("_TessDistMax", tessDistMax);
    }

    public float GetUVRadius(float worldRadius) => worldRadius * worldToUVScale;

    public void PaintGrowth(Vector2 uv, float worldRadius, float strength)
    {
        float uvRadius = worldRadius * worldToUVScale;
        int mode = strength > 0 ? BrushPainter.MODE_GROWTH_ADD : BrushPainter.MODE_GROWTH_SUBTRACT;
        BrushPainter.Paint(woolRT, uv, uvRadius, Mathf.Abs(strength), Color.clear, mode);
    }

    public void PaintColor(Vector2 uv, float worldRadius, float strength, Color color)
    {
        float uvRadius = worldRadius * worldToUVScale;
        BrushPainter.Paint(woolRT, uv, uvRadius, strength, color, BrushPainter.MODE_COLOR);
    }

    private void OnDestroy()
    {
        if (woolRT != null) { woolRT.Release(); Destroy(woolRT); }
        if (inflatedColliderMesh != null) Destroy(inflatedColliderMesh);
    }
}
