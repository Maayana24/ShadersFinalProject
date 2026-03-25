using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private Sprite cursor;
    [SerializeField] protected float radius = 0.15f;
    [SerializeField] protected float strength = 0.5f;

    public Sprite Cursor => cursor;

    public abstract Particles Type { get; protected set; }
    public abstract void ApplyEffect(Vector3 worldHitPosition, Vector2 uv, SheepWoolManager woolManager);
}