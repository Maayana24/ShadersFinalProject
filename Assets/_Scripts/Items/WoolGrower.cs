using UnityEngine;

public class WoolGrower : Item
{
    public override void ApplyEffect(Vector3 worldHitPosition, GameObject hitObject)
    {
        Debug.Log("Wool Grower!");
    }
}