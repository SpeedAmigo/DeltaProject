using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class TerrainGenerator : MonoBehaviour
{
    [Header("Generation Seed")]
    public int seed;
    [Space(10)]
    
    [Header("Grid Size")]
    public Vector3Int gridSize = new(100, 0, 100);
    [Space(5)]
    
    [Header("General Settings")]
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private List<Layer> materialLayers = new();
    [SerializeField] private AnimationCurve noiseCurve;
    [SerializeField] private float scale = 1f;
    [SerializeField] private int width;
    [SerializeField] private int depth;
    [SerializeField] private float heightMultiplier;
    [SerializeField] private float xPosOffset;
    [SerializeField] private float zPosOffset;
    [SerializeField] private bool enableFallOff;
    
    [SerializeField] private bool useClamping;
    [ShowIf("useClamping")]
    [SerializeField] private float minClamp = 0f;
    [ShowIf("useClamping")]
    [SerializeField] private float maxClamp = 5f;
    
    [Header("Noise Settings")]
    [SerializeField] private float perlinScale;
    [SerializeField] private int octaves = 1;
    [SerializeField] private float lacunarity = 2f;
    [SerializeField] private float persistence = 0.5f;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private System.Random prng;
    private float xPerlinOffset;
    private float zPerlinOffset;
    
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
        prng = new System.Random(seed);

        xPerlinOffset = prng.Next(-100000, 100000);
        zPerlinOffset = prng.Next(-100000, 100000);
        
        ClearMesh();
        
        int halfX = gridSize.x / 2;
        int halfZ = gridSize.z / 2;
        
        for (int z = -halfZ; z < gridSize.z - halfZ; z++)
        {
            for (int x = -halfX; x < gridSize.x - halfX; x++)
            {
                Vector3 chunkOffset = new Vector3(x * xPosOffset, 0f, z * zPosOffset);
                
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

                float worldPosX = position.x + x * scale;
                float worldPosZ = position.z + z * scale;

                for (int o = 0; o < octaves; o++)
                {
                    float frequency = Mathf.Pow(lacunarity, o);
                    float amplitude = Mathf.Pow(persistence, o);

                    float sampleX = (x + position.x + xPerlinOffset) / perlinScale * frequency;
                    float sampleZ = (z + position.z + zPerlinOffset) / perlinScale * frequency;
                    
                    yPos += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
                }

                yPos *= noiseCurve.Evaluate(yPos);

                if (useClamping)
                {
                    yPos = Mathf.Clamp(yPos, minClamp, maxClamp);
                }
                
                if (enableFallOff)
                {
                    float falloff = EvaluateGlobalFallOff(worldPosX, worldPosZ);
                    yPos -= falloff;

                    if (useClamping)
                    {
                        yPos = Mathf.Clamp(yPos, minClamp, maxClamp);
                    }
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
        
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int u = 0; u < uvs.Length; u++)
        {
            uvs[u] = new Vector2(vertices[u].x, vertices[u].z);
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }

    private float EvaluateGlobalFallOff(float worldX, float worldZ)
    {
        float maxDistance = (gridSize.x * xPosOffset) / 2f;
        float distance = new Vector2(worldX, worldZ).magnitude;
        
        float normalizedDistance = distance / maxDistance;
        normalizedDistance = Mathf.Clamp01(normalizedDistance);

        float a = 3f; //Steepness
        float b = 2.2f; // Curve sharpness
        
        return Mathf.Pow(normalizedDistance, a) / (Mathf.Pow(normalizedDistance, a) + Mathf.Pow(b - b * normalizedDistance, a));
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
}

[System.Serializable]
public class Layer
{
    public Texture2D texture;
    [Range(0f,1f)] public float startHeight;
}