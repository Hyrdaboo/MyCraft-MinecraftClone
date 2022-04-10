using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkConstructor : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public Vector2 offset;
    public Texture textureAtlas;
    
    public List<Vector3Int> placedBlocks = new List<Vector3Int>();
    public List<Vector3Int> removedBlocks = new List<Vector3Int>();
    public Dictionary<Vector3Int, BlockType> placedBlocksType = new Dictionary<Vector3Int, BlockType>();

    public int id = 0;
    public float value3D(FastNoise fn, int x, int y, int z)
    {
        return mapGenerator.Generate3D(fn, offset, x, y, z, 1.2f);
    }

    public float mask3D(FastNoise fn, int x, int y, int z)
    {
        return mapGenerator.Generate3D(fn, offset, x, y, z, .1f);
    }
    public float stoneMask(FastNoise fn, int x, int y)
    {
        return mapGenerator.GenerateMask(fn, offset, x, y);
    }
    public Mesh DrawMesh()
    {
        float[,] heightmap = mapGenerator.GenerateHeightMap(offset);
        
        MeshDataGenerator mdGen = new MeshDataGenerator();
        MeshData meshData = mdGen.GenerateMeshData(mapGenerator.width, 256, heightmap, this);
        return meshData.ConstructMesh();
    }
}
