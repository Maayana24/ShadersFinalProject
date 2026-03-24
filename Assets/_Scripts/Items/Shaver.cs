using UnityEngine;

public class Shaver : Item
{
    public override void ApplyEffect(Vector3 worldHitPosition, GameObject hitObject)
    {
        Debug.Log("Shaver!");
    }
}