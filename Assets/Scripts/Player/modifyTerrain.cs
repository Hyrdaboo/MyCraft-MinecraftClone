using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class modifyTerrain : MonoBehaviour
{
    enum ModifyMode { place, remove }
    public Transform indicator;
    public Camera playerCam;
    public LayerMask ground;

    private GameObject activeChunk;
    private ChunkConstructor constructor;

    public Vector3Int breakPos;
    public Vector3Int placePos;
    public BlockType blockToPlace;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) RemoveBlock(breakPos);
        if (Input.GetMouseButtonDown(1)) PlaceBlock(placePos);
        if (Input.GetKeyDown(KeyCode.Backspace)) regen();
        GetActiveChunk();
    }

    public void GetActiveChunk()
    {
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 5, ground))
        {
            setActiveBlockPos(hit.point, hit.normal);
            activeChunk = hit.transform.gameObject;
            constructor = activeChunk.GetComponent<ChunkConstructor>();
        }
        else
        {
            activeChunk = null;
            placeIndicator(Vector3.zero);
        }
    }

    float refreshRate = .1f;
    float nextRefresh = 0;
    public void setActiveBlockPos(Vector3 hitPos, Vector3 hitnormal)
    {
        
        bool forw = hitnormal == Vector3.left || hitnormal == Vector3.back;
        bool back = hitnormal == Vector3.right || hitnormal == Vector3.forward;

        if (forw)
        {
            hitPos = new Vector3(hitPos.x, hitPos.y+0.5f, hitPos.z);
        }
        if (back)
        {
            hitPos = new Vector3(hitPos.x-.001f, hitPos.y + 0.5f, hitPos.z-.001f);
        }
        if (hitnormal == Vector3.down)
        {
            hitPos = new Vector3(hitPos.x, hitPos.y + 1, hitPos.z);
        }

        Vector3Int posInGrid = Vector3Int.FloorToInt(hitPos);

        if (Time.time > nextRefresh)
        {
            nextRefresh = Time.time + refreshRate;
            placeIndicator(posInGrid);
        }
        breakPos = worldToLocal(posInGrid);
        placePos = worldToLocal(posInGrid+ Vector3Int.RoundToInt(hitnormal));
    }

    void placeIndicator(Vector3 pos)
    {
        indicator.transform.position = pos;
    }

    Vector3Int worldToLocal(Vector3Int world)
    {
        if (activeChunk == null) return Vector3Int.zero;
        Vector3Int chunkPos = Vector3Int.RoundToInt(activeChunk.transform.position);
        return AbsVector3Int(AbsVector3Int(world) - AbsVector3Int(chunkPos));
    }

    Vector3Int AbsVector3Int(Vector3Int vec)
    {
        return new Vector3Int(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }

    private void RemoveBlock(Vector3Int pos)
    {
        ModifyBlock(pos, ModifyMode.remove);
    }
    private void PlaceBlock(Vector3Int pos)
    {
        ModifyBlock(pos, ModifyMode.place);
    }

    private void ModifyBlock(Vector3Int pos, ModifyMode modifyMode)
    {
        if (activeChunk == null || constructor == null) return;

        if (modifyMode == ModifyMode.place)
        {
            if (constructor.placedBlocks.Contains(pos)) return;
            if (constructor.removedBlocks.Contains(pos)) constructor.removedBlocks.Remove(pos);
            constructor.placedBlocks.Add(pos);
            if (constructor.placedBlocksType.ContainsKey(pos))
            {
                constructor.placedBlocksType[pos] = blockToPlace;
            }
            else
            {
                constructor.placedBlocksType.Add(pos, blockToPlace);
            }
        }
        else
        {
            if (constructor.removedBlocks.Contains(pos)) return;
            if (constructor.placedBlocks.Contains(pos)) constructor.placedBlocks.Remove(pos);
            constructor.removedBlocks.Add(pos);
        }

        MeshFilter mf = activeChunk.GetComponent<MeshFilter>();
        mf.mesh = constructor.DrawMesh();
        activeChunk.GetComponent<MeshCollider>().sharedMesh = mf.mesh;
    }

    void regen()
    {
        if (activeChunk == null) return;
        MeshFilter mf = activeChunk.GetComponent<MeshFilter>();
        constructor.placedBlocks.Clear();
        mf.mesh = constructor.DrawMesh();
        activeChunk.GetComponent<MeshCollider>().sharedMesh = mf.mesh;
    }
}
