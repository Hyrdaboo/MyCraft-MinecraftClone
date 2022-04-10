using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public static bool finished = false;
    public static float maxViewDst = 50;
    public MapGenerator mapGenerator;
    public Material chunkMaterial;
    public static Vector2 viewerPosition;
    public Transform viewer;

    private int chunkSize;
    private int chunksVisibleInView;

    private Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> chunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        chunkSize = mapGenerator.width-2;
        chunksVisibleInView = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateChunks();
    }

    public void UpdateChunks()
    { 
        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++)
        {
            chunksVisibleLastUpdate[i].SetVisible(false);
        }
        chunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int offsetY = -chunksVisibleInView; offsetY < chunksVisibleInView; offsetY++)
        {
            for (int offsetX = -chunksVisibleInView; offsetX < chunksVisibleInView; offsetX++)
            {
                Vector2 viewChunkCoord = new Vector2(currentChunkCoordX + offsetX, currentChunkCoordY + offsetY);

                if (chunkDictionary.ContainsKey(viewChunkCoord))
                {
                    chunkDictionary[viewChunkCoord].UpdateTerrainChunk();
                    if (chunkDictionary[viewChunkCoord].isVisible())
                    {
                        chunksVisibleLastUpdate.Add(chunkDictionary[viewChunkCoord]);
                    }
                }
                else
                {
                    TerrainChunk newChunk = new TerrainChunk(this, viewChunkCoord, chunkSize, transform);
                    chunkDictionary.Add(viewChunkCoord, newChunk);
                }
            }
        }
    }

    private void DrawChunk(GameObject chunk, Vector2 offset)
    {
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        ChunkConstructor chunkConstructor = chunk.AddComponent<ChunkConstructor>();
        MeshCollider collider = chunk.AddComponent<MeshCollider>();

        chunkConstructor.mapGenerator = mapGenerator;
        chunkConstructor.offset = offset;
        chunkConstructor.textureAtlas = chunkMaterial.mainTexture;
        meshFilter.mesh = chunkConstructor.DrawMesh();
        collider.sharedMesh = meshFilter.mesh;
        meshRenderer.sharedMaterial = chunkMaterial;
        finished = true;
    }

    public class TerrainChunk
    {
        public GameObject chunkBody;
        private Vector2 position;
        private Bounds bounds;

        public TerrainChunk(EndlessTerrain generator, Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(coord, Vector2.one * size);
            Vector3 pos = new Vector3(position.x, 0, position.y);

            chunkBody = new GameObject();  
            chunkBody.name = "Chunk";
            chunkBody.transform.position = pos;
            chunkBody.transform.parent = parent;
            chunkBody.layer = 3;
            generator.DrawChunk(chunkBody, new Vector2(position.x, -position.y));
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewDstFromNearestEdge <= maxViewDst * 100000;
            SetVisible(visible);
        }
        public void SetVisible(bool visible)
        {
            chunkBody.SetActive(visible);
        }
        public bool isVisible()
        {
            return chunkBody.activeSelf;
        }
    }
}
