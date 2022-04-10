using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshDataGenerator
{
    static FastNoise fn = null;

    int randX = 0;
    int randY = 0;
    int randTreeLength = 0;
    public MeshData GenerateMeshData(int width, int height, float[,] heightmap, ChunkConstructor cc)
    {
        MeshData data = new MeshData(width, height);
        if (fn == null)
        {
            fn = new FastNoise();
        }
        if (cc.id == 0) cc.id = new System.Random().Next();
        randTreeLength = new System.Random(cc.id).Next(8, 16);
        
        randX = getRandomTreePos().x;
        randY = getRandomTreePos().y;

        data.texSize = 16;
        data.atlasSize = new Vector2(cc.textureAtlas.width, cc.textureAtlas.height);
        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < width; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noiseHeight = heightmap[x, z];
                    float height3D = cc.value3D(fn, x, y, z);
                    float mask = cc.mask3D(fn, x, y, z);
                    float stoneMask = cc.stoneMask(fn, x, z);

                    if (y < (int)noiseHeight)
                    {
                        data.blocks[x, y, z].solid = true;
                    }
                    if (height3D < 0f && y < (int)noiseHeight && mask > .5f)
                    {
                        data.blocks[x, y, z].solid = false;
                        data.blocks[x, y, z].blockType = BlockType.undefined;
                    }
                    if (mask < .75f && y < (int)noiseHeight)
                    {
                        data.blocks[x, y, z].solid = true;
                    }
                    if (y < (int)noiseHeight - 1)
                    {
                        data.blocks[x, y, z].blockType = BlockType.dirt;
                    }
                    stoneMask = Mathf.Clamp(stoneMask, .1f, 1);
                    if (y < noiseHeight-40*stoneMask)
                    {
                        data.blocks[x, y, z].blockType = BlockType.stone;
                    }
                    if (cc.placedBlocksType.ContainsKey(new Vector3Int(x, y, z)))
                    {
                        data.blocks[x, y, z].blockType = cc.placedBlocksType[new Vector3Int(x, y, z)];
                    }
                    if (y < (int)noiseHeight && y > (int)noiseHeight-2)
                    {
                        data.blocks[x, y, z].blockType = BlockType.grass;
                    }
                    bool validTreePos = data.blocks[x, y, z].blockType == BlockType.grass 
                                        && data.blocks[x, y, z].solid == true 
                                        && x == randX && z == randY;
                    if (validTreePos)
                    {
                        int maxPoint = 0;
                        for (int i = 1; i < randTreeLength; i++)
                        {
                            SetTreeBlock(x, y + i, z, BlockType.log);
                            maxPoint = y+i;
                        }

                        for (int _z = z-2; _z < (z-2)+5; _z++)
                        {
                            for (int _x = x-2; _x < (x-2)+5; _x++)
                            {
                                if (_x > 15 || _z > 15 || _x < 1 || _z < 1) continue;
                                SetTreeBlock(_x, maxPoint, _z, BlockType.leave);
                            }
                        }
                        for (int _z = z - 1; _z < (z - 1) + 3; _z++)
                        {
                            for (int _x = x - 1; _x < (x - 1) + 3; _x++)
                            {
                                if (_x > 15 || _z > 15 || _x < 1 || _z < 1) continue;
                                SetTreeBlock(_x, maxPoint+1, _z, BlockType.leave);
                            }
                        }
                    }

                    data.blocks[x, y, z].AssignTexture();
                }
            }
        }

        for (int i = 0; i < cc.placedBlocks.Count; i++)
        {
            Vector3Int pos = cc.placedBlocks[i];
            data.blocks[pos.x, pos.y, pos.z].solid = true;
        }

        for (int i = 0; i < cc.removedBlocks.Count; i++)
        {
            Vector3Int pos = cc.removedBlocks[i];
            data.blocks[pos.x, pos.y, pos.z].solid = false;
        }

        Vector2Int getRandomTreePos()
        {
            System.Random rand = new System.Random(0);
            
            return new Vector2Int(rand.Next(1, 15), rand.Next(1, 15));
        }

        void SetTreeBlock(int x, int y, int z, BlockType type)
        {
            data.blocks[x, y, z].solid = true;
            data.blocks[x, y, z].blockType = type;
        }

        data.ConstructBlocks();
        return data;
    }
}

public class MeshData
{
    public Block[,,] blocks;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int> ();
    private List<Vector2> uvs = new List<Vector2>();

    public float texSize = 0; // set from mesh data generator
    public Vector2 atlasSize = Vector2.zero;

    private int triangleIndex = 0;
    private int vertexIndex = 0;

    private int width;
    private int height;
    public MeshData(int width, int height)
    {
        //Debug.Log(width);
        this.width = width;
        this.height = height;
        blocks = new Block[this.width, this.height, this.width];
        for (int y = 0; y < this.height; y++)
        {
            for (int z = 0; z < this.width; z++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    blocks[x, y, z] = new Block(new Vector3Int(x, y, z));
                    blocks[x, y, z].blockType = BlockType.undefined;
                }
            }
        }
    }

    public void ConstructBlocks()
    {
        foreach (Block block in blocks)
        {
            bool isEdge = block.position.x == 0 || block.position.x == width - 1 
                          || block.position.z == 0 || block.position.z == width - 1 || 
                          block.position.y == height - 1 || block.position.y == 0;
            if(!block.solid)
            {
                continue;
            }
            else
            {
                if (!isEdge) CheckNeighbors(block);
            }
        }
    }

    private void CheckNeighbors(Block block)
    {
        Vector3Int current = block.position;
        
        Block top = blocks[current.x, current.y + 1, current.z];
        Block bottom = blocks[current.x, current.y - 1, current.z];
        Block front = blocks[current.x, current.y, current.z - 1];
        Block back = blocks[current.x, current.y, current.z+1];
        Block right = blocks[current.x - 1, current.y, current.z]; 
        Block left = blocks[current.x + 1, current.y, current.z];


        // if neighbor is air construct face
        if (!top.solid)
        {
            ConstructFace(block, current, top.position);
        }
        if (bottom != null && !bottom.solid)
        {
            ConstructFace(block, current, bottom.position);
        }
        if (front != null && !front.solid)
        {
            ConstructFace(block, current, front.position);
        }
        if (back != null && !back.solid)
        {
            ConstructFace(block, current, back.position);
        }
        if (right != null && !right.solid)
        {
            ConstructFace(block, current, right.position);
        }
        if (left != null && !left.solid)
        {
            ConstructFace(block, current, left.position);
        }
    }

    private void ConstructFace(Block block, Vector3Int cur, Vector3Int neighbor)
    {
        bool top = neighbor.y > cur.y;
        bool bottom = neighbor.y < cur.y;
        bool front = neighbor.z < cur.z;
        bool back = neighbor.z > cur.z;
        bool right = neighbor.x > cur.x; 
        bool left = neighbor.x < cur.x;

        // for each side add its vertices and triangles
        if (top)
        {
            AddVertex(cur.x, cur.y + 0.5f, cur.z);
            AddVertex(cur.x, cur.y + 0.5f, cur.z + 1);
            AddVertex(cur.x + 1, cur.y + 0.5f, cur.z);
            AddVertex(cur.x + 1, cur.y + 0.5f, cur.z + 1);

            AddTriangle(0 + 4*vertexIndex, 1 + 4*vertexIndex, 3 + 4*vertexIndex);
            AddTriangle(3 + 4*vertexIndex, 2 + 4*vertexIndex, 0 + 4*vertexIndex);

            AddUv(block.topTextureCoord);
            vertexIndex++;
        }
        if  (bottom)
        {
            AddVertex(cur.x, cur.y - 0.5f, cur.z);
            AddVertex(cur.x, cur.y - 0.5f, cur.z + 1);
            AddVertex(cur.x + 1, cur.y - 0.5f, cur.z);
            AddVertex(cur.x + 1, cur.y - 0.5f, cur.z + 1);

            AddTriangle(0 + 4 * vertexIndex, 2 + 4 * vertexIndex, 3 + 4 * vertexIndex);
            AddTriangle(3 + 4 * vertexIndex, 1 + 4 * vertexIndex, 0 + 4 * vertexIndex);

            AddUv(block.bottomTextureCoord);
            vertexIndex++;
        }
        if (front)
        {
            AddVertex(cur.x, cur.y-0.5f, cur.z);
            AddVertex(cur.x+1, cur.y-0.5f, cur.z);
            AddVertex(cur.x, cur.y + 0.5f, cur.z);
            AddVertex(cur.x + 1, cur.y + 0.5f, cur.z);

            AddTriangle(0 + 4 * vertexIndex, 2 + 4 * vertexIndex, 3 + 4 * vertexIndex);
            AddTriangle(3 + 4 * vertexIndex, 1 + 4 * vertexIndex, 0 + 4 * vertexIndex);

            AddUv(block.sideTextureCoord);
            vertexIndex++;
        }
        if (back)
        {
            AddVertex(cur.x, cur.y - 0.5f, cur.z+1);
            AddVertex(cur.x + 1, cur.y - 0.5f, cur.z+1);
            AddVertex(cur.x, cur.y + 0.5f, cur.z+1);
            AddVertex(cur.x + 1, cur.y + 0.5f, cur.z+1);

            AddTriangle(0 + 4 * vertexIndex, 1 + 4 * vertexIndex, 3 + 4 * vertexIndex);
            AddTriangle(3 + 4 * vertexIndex, 2 + 4 * vertexIndex, 0 + 4 * vertexIndex);

            AddUv(block.sideTextureCoord);
            vertexIndex++;
        }
        if (right)
        {
            AddVertex(cur.x + 1, cur.y-0.5f, cur.z);
            AddVertex(cur.x + 1, cur.y-0.5f, cur.z + 1);
            AddVertex(cur.x + 1, cur.y + 0.5f, cur.z);
            AddVertex(cur.x + 1, cur.y + 0.5f, cur.z + 1);

            AddTriangle(0 + 4 * vertexIndex, 2 + 4 * vertexIndex, 3 + 4 * vertexIndex);
            AddTriangle(3 + 4 * vertexIndex, 1 + 4 * vertexIndex, 0 + 4 * vertexIndex);

            AddUv(block.sideTextureCoord);
            vertexIndex++;
        }
        if (left)
        {
            AddVertex(cur.x, cur.y - 0.5f, cur.z);
            AddVertex(cur.x, cur.y - 0.5f, cur.z + 1);
            AddVertex(cur.x, cur.y + 0.5f, cur.z);
            AddVertex(cur.x, cur.y + 0.5f, cur.z + 1);

            AddTriangle(0 + 4 * vertexIndex, 1 + 4 * vertexIndex, 3 + 4 * vertexIndex);
            AddTriangle(3 + 4 * vertexIndex, 2 + 4 * vertexIndex, 0 + 4 * vertexIndex);

            AddUv(block.sideTextureCoord);
            vertexIndex++;
        }
    }

    private void AddVertex(float a, float b, float c)
    {
        vertices.Add(new Vector3(a,b,c));
    }

    private void AddTriangle(int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);

        triangleIndex += 3;
    }

    private void AddUv (Vector2 texCoord)
    {
        uvs.Add(texCoord * texSize / atlasSize);
        uvs.Add(new Vector2(texCoord.x + 1, texCoord.y) * texSize / atlasSize);
        uvs.Add(new Vector2(texCoord.x, texCoord.y + 1) * texSize / atlasSize);
        uvs.Add(new Vector2(texCoord.x + 1, texCoord.y + 1) * texSize / atlasSize);
    }

    public Mesh ConstructMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Chunk";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
}

public enum BlockType { grass, dirt, stone, log, leave, plank, brick, glass, cobblestone, undefined }

public class Block
{
    public Vector2 topTextureCoord = Vector2.one;
    public Vector2 sideTextureCoord = Vector2.one;
    public Vector2 bottomTextureCoord = Vector2.one;

    public Vector3Int position;
    public bool solid = false;
    public BlockType blockType;

    public Block(Vector3Int position)
    {
        this.position = position;
    }

    public void AssignTexture()
    {
        switch (blockType)
        {
            case BlockType.grass:
                topTextureCoord = new Vector2(1, 2);
                sideTextureCoord = new Vector2(0, 1);
                bottomTextureCoord = new Vector2(0, 0);
                break;
            case BlockType.stone:
                topTextureCoord = new Vector2(0, 2);
                sideTextureCoord = new Vector2(0, 2);
                bottomTextureCoord = new Vector2(0, 2);
                break;
            case BlockType.cobblestone:
                topTextureCoord = new Vector2(3, 1);
                sideTextureCoord = new Vector2(3, 1);
                bottomTextureCoord = new Vector2(3, 1);
                break;
            case BlockType.log:
                topTextureCoord = new Vector2(1, 0);
                sideTextureCoord = new Vector2(1, 1);
                bottomTextureCoord = new Vector2(1, 0);
                break;
            case BlockType.plank:
                topTextureCoord = new Vector2(2, 1);
                sideTextureCoord = new Vector2(2, 1);
                bottomTextureCoord = new Vector2(2, 1);
                break;
            case BlockType.brick:
                topTextureCoord = new Vector2(2, 2);
                sideTextureCoord = new Vector2(2, 2);
                bottomTextureCoord = new Vector2(2, 2);
                break;
            case BlockType.glass:
                topTextureCoord = new Vector2(3, 0);
                sideTextureCoord = new Vector2(3, 0);
                bottomTextureCoord = new Vector2(3, 0);
                break;
            case BlockType.leave:
                topTextureCoord = new Vector2(2, 0);
                sideTextureCoord = new Vector2(2, 0);
                bottomTextureCoord = new Vector2(2, 0);
                break;
            case BlockType.undefined:
                topTextureCoord = new Vector2(15, 15);
                sideTextureCoord = new Vector2(15, 15);
                bottomTextureCoord = new Vector2(15, 15);
                break;
            case BlockType.dirt:
                topTextureCoord = Vector2.zero;
                sideTextureCoord = Vector2.zero;
                bottomTextureCoord = Vector2.zero;
                break;
            default:
                topTextureCoord = Vector2.one;
                sideTextureCoord = Vector2.one;
                bottomTextureCoord = Vector2.one;
                break;
        }
    }
}