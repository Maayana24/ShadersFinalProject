using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private Sprite cursor;

    public Sprite Cursor => cursor;

    public abstract Particles Type { get; protected set; }
    public abstract void ApplyEffect(Vector3 worldHitPosition, Vector2 uv, SheepWoolManager woolManager, float radius, float strength);
}
