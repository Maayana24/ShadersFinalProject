using UnityEngine;

public class WoolGrower : Item
{
    public override Particles Type { get; protected set; } = Particles.Sparkles;
    public override void ApplyEffect(Vector3 worldHitPosition, GameObject hitObject)
    {
        Debug.Log("Wool Grower!");
    }
}