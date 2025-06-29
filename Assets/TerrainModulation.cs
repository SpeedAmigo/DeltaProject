using UnityEngine;

public class TerrainModulation : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;
    
    private Mesh mesh;
    private Vector3[] vertices;

    public void ModulateTerrain(Vector3 position, float height, float range)
    {
        mesh = meshFilter.mesh;
        vertices = mesh.vertices;
        position -= meshFilter.transform.position;

        int i = 0;
        foreach (Vector3 vertex in vertices) 
        {
            if (Vector2.Distance(new Vector2(vertex.x, vertex.z), new Vector2(position.x, position.z)) <= range)
            {
                vertices[i] = vertex + new Vector3(0f, height, 0f);
            }
            i++;
        }
        
        mesh.vertices = vertices;
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}