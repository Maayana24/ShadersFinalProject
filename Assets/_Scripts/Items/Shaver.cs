using UnityEngine;

public class Shaver : Item
{
    public override Particles Type { get; protected set; } = Particles.HairCutting;
    public override void ApplyEffect(Vector3 worldHitPosition, Vector2 uv, SheepWoolManager woolManager)
    {
        Debug.Log($"Shaver! worldPos={worldHitPosition}, uv={uv}");
        woolManager.PaintGrowth(uv, radius, -strength * Time.deltaTime);
    }
}