using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private bool showVertices;
    
    public Vector3Int gridSize = new(100, 0, 100);
    
    public Material terrainMaterial;
    
    public float scale = 1f;
    public int width;
    public int depth;
    public float perlinScale;
    public float heightMultiplier;
    
    public float xOffset;
    public float zOffset;

    public int octaves = 1;
    public float lacunarity = 2f;
    public float persistence = 0.5f;
    

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    
    private void Start()
    {
        GenerateChunkGrid(gridSize);
    }

    [Button("Generate Chunk Grid")] [HorizontalGroup] [GUIColor("Green")]
    private void GenerateTerrain()
    {
        GenerateChunkGrid(gridSize);
    }
    
    private void GenerateChunkGrid(Vector3Int gridSize)
    {
        ClearMesh();

        for (int z = 0; z < gridSize.z; z++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                Vector3 chunkOffset = new Vector3(x * xOffset, 0f, z * zOffset);
                
                Mesh chunkMesh = GenerateMesh(chunkOffset);
                GameObject chunkObject = CreateMeshObject(chunkMesh, chunkOffset);
                chunkObject.transform.SetParent(transform);
                chunkObject.name = $"Chunk_{x}_{z}";
            }
        }
    }
    

    [Button] [HorizontalGroup] [GUIColor("Red")]
    private void ClearMesh()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }
    
    private Mesh GenerateMesh(Vector3 position)
    {
        mesh = new Mesh();
        
        vertices = new Vector3[(width + 1) * (depth + 1)];

        int i = 0;
        
        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float yPos = 0;

                for (int o = 0; o < octaves; o++)
                {
                    float frequency = Mathf.Pow(lacunarity, o);
                    float amplitude = Mathf.Pow(persistence, o);
                    
                    yPos += Mathf.PerlinNoise((x + position.x) / perlinScale * frequency, (z + position.z) / perlinScale * frequency) * amplitude;
                }
                
                yPos *= heightMultiplier;
                
                vertices[i] = new Vector3(scale * x, yPos, scale * z);
                i++;
            }
        }
        
        triangles = new int[width * depth * 6];
        
        int vertex = 0;
        int triangleIndex = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[triangleIndex + 0] = vertex + 0;
                triangles[triangleIndex + 1] = vertex + width + 1;
                triangles[triangleIndex + 2] = vertex + 1;
                
                triangles[triangleIndex + 3] = vertex + 1;
                triangles[triangleIndex + 4] = vertex + width + 1;
                triangles[triangleIndex + 5] = vertex + width + 2;
                
                vertex++;
                triangleIndex += 6;
            }

            vertex++;
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }

    private GameObject CreateMeshObject(Mesh mesh, Vector3 position)
    {
        GameObject obj = new GameObject("Chunk");
        obj.AddComponent<MeshFilter>().sharedMesh = mesh;;
        obj.AddComponent<MeshRenderer>().material = terrainMaterial;
        obj.AddComponent<MeshCollider>().sharedMesh = mesh;
        
        obj.transform.position = position;
        
        return obj;
    }
    
    #region assetSave
#if UNITY_EDITOR
    [Button("Generate & Save Prefab")] [GUIColor("Blue")]
    private void GenerateAndSaveAsPrefab()
    {
        //GenerateMesh(); // make sure we generate fresh

        // Ensure folders exist
        Directory.CreateDirectory("Assets/SavedMeshes");
        Directory.CreateDirectory("Assets/Prefabs");

        // Save mesh asset
        string meshPath = $"Assets/SavedMeshes/{name}_Mesh.asset";
        AssetDatabase.CreateAsset(Object.Instantiate(mesh), meshPath);
        AssetDatabase.SaveAssets();

        // Create GameObject using the saved mesh
        GameObject tempGO = new GameObject($"{name}_Generated");
        MeshFilter mf = tempGO.AddComponent<MeshFilter>();
        mf.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        tempGO.AddComponent<MeshRenderer>();
        MeshCollider col = tempGO.AddComponent<MeshCollider>();
        col.sharedMesh = mf.sharedMesh;

        // Save as prefab
        string prefabPath = $"Assets/Prefabs/{name}_Prefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(tempGO, prefabPath);

        // Cleanup
        DestroyImmediate(tempGO);

        Debug.Log($"Mesh saved to: {meshPath}\nPrefab saved to: {prefabPath}");
    }
#endif
    #endregion
    
    private void OnDrawGizmos()
    {
        if (vertices != null && showVertices)
        {
            foreach (Vector3 pos in vertices)
            {
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }
}
