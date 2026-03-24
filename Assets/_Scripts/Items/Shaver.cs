using UnityEngine;

public class Shaver : Item
{
    public override Particles Type { get; protected set; } = Particles.HairCutting;
    public override void ApplyEffect(Vector3 worldHitPosition, GameObject hitObject)
    {
        Debug.Log("Shaver!");
    }
}