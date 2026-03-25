using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class HairProxyCollider : MonoBehaviour
{
    [SerializeField] private MeshFilter hairMeshFilter;
    [SerializeField] private float maxHairLength = 0.1f;
    [SerializeField] private MeshCollider targetMeshCollider;

    private void Awake()
    {
        BuildProxyMesh();
    }

    private void BuildProxyMesh()
    {
        Mesh original = hairMeshFilter.sharedMesh;

        Vector3[] vertices = original.vertices;
        Vector3[] normals = original.normals;
        Vector3[] inflated = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            inflated[i] = vertices[i] + normals[i] * maxHairLength;

        Mesh proxy = new Mesh
        {
            name = "HairProxyCollider",
            vertices = inflated,
            triangles = original.triangles,
            uv = original.uv
        };
        proxy.RecalculateBounds();

        targetMeshCollider.sharedMesh = proxy;
    }
}
