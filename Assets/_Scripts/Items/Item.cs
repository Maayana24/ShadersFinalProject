using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private Sprite cursor;
    [SerializeField] protected float radius = 0.1f;

    public Sprite Cursor => cursor;

    public abstract void ApplyEffect(Vector3 worldHitPosition, GameObject hitObject);
}