using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private Sprite cursor;

    public Sprite Cursor => cursor;

    public abstract void ApplyEffect(Vector3 worldHitPosition, GameObject hitObject);
}