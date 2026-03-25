using UnityEngine;

public class WoolGrower : Item
{
    public override Particles Type { get; protected set; } = Particles.Sparkles;
    public override void ApplyEffect(Vector3 worldHitPosition, Vector2 uv, SheepWoolManager woolManager, float radius, float strength)
    {
        woolManager.PaintGrowth(uv, radius, strength * Time.deltaTime);
    }
}
