using UnityEngine;

public static class BrushPainter
{
    private static Material brushMat;

    private static Material GetBrushMaterial()
    {
        if (brushMat == null)
            brushMat = new Material(Shader.Find("Hidden/BrushPaint"));
        return brushMat;
    }

    public static void Paint(RenderTexture target, Vector2 uvCenter, float uvRadius, float strength, Color color, int mode)
    {
        Material mat = GetBrushMaterial();
        mat.SetVector("_BrushCenter", uvCenter);
        mat.SetFloat("_BrushRadius", uvRadius);
        mat.SetFloat("_BrushStrength", strength);
        mat.SetColor("_BrushColor", color);
        mat.SetInt("_PaintMode", mode);

        RenderTexture temp = RenderTexture.GetTemporary(target.descriptor);
        Graphics.Blit(target, temp);
        mat.SetTexture("_BaseTexture", temp);
        Graphics.Blit(temp, target, mat);
        RenderTexture.ReleaseTemporary(temp);
    }

    public const int MODE_GROWTH_ADD = 0;
    public const int MODE_GROWTH_SUBTRACT = 1;
    public const int MODE_COLOR = 2;
}
