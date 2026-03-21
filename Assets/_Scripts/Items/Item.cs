using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField] private GameObject cursorPrefab;
    [SerializeField] private float distanceFromCamera = 2;

    public GameObject CursorPrefab => cursorPrefab;
    public float DistanceFromCamera => distanceFromCamera;

    public abstract void ApplyEffect(Vector3 worldHitPosition, GameObject hitObject);
}