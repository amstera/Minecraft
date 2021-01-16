using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord Coord;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private GameObject _chunkObject;

    private int _vertexIndex;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();
    private World _world;
    private bool _isActive;

    public byte[,,] VoxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public Queue<VoxelMod> Modifications = new Queue<VoxelMod>();
    public bool IsVoxelMapPopulated;

    public Chunk(ChunkCoord coord, World world, bool generateOnLoad)
    {
        Coord = coord;
        _world = world;
        _isActive = true;
        if (generateOnLoad)
        {
            Init();
        }
    }

    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (_chunkObject != null)
            {
                _chunkObject.SetActive(value);
            }
        }
    }

    public Vector3 Position
    {
        get { return _chunkObject.transform.position; }
    }

    public void Init()
    {
        _chunkObject = new GameObject();
        _chunkObject.transform.SetParent(_world.transform);
        _chunkObject.transform.position = new Vector3(Coord.x * VoxelData.ChunkWidth, 0, Coord.z * VoxelData.ChunkWidth);
        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        _meshRenderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = _world.Material;

        PopulateVoxelMap();

        UpdateChunk();
    }

    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);

        return VoxelMap[xCheck, yCheck, zCheck];
    }

    public Blocks GetBlockFromGlobalVector3(Vector3 pos)
    {
        return (Blocks)GetVoxelFromGlobalVector3(pos);
    }

    public void EditVoxel(Vector3 pos, Blocks block)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);

        VoxelMap[xCheck, yCheck, zCheck] = (byte)block;

        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);

        UpdateChunk();
    }

    private void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 voxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = voxel + VoxelData.FaceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                _world.GetChunkFromVector3(voxel + Position).UpdateChunk();
            }
        }
    }

    private void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    VoxelMap[x, y, z] = _world.GetVoxel(new Vector3(x, y, z) + Position);
                }
            }
        }

        IsVoxelMapPopulated = true;
    }

    private bool IsVoxelInChunk(int x, int y, int z)
    {
        return !(x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1);
    }

    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return _world.CheckForVoxel(pos + Position);
        }

        return _world.BlockTypes[VoxelMap[x, y, z]].IsSolid;
    }

    public void UpdateChunk()
    {
        while (Modifications.Count > 0)
        {
            VoxelMod v = Modifications.Dequeue();
            Vector3 pos = v.Position -= Position;
            VoxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = (byte)v.BlockId;
        }

        ClearMeshData();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (_world.BlockTypes[VoxelMap[x, y, z]].IsSolid)
                    {
                        UpdateMeshData(new Vector3(x, y, z));
                    }
                }
            }
        }

        Mesh mesh = new Mesh
        {
            vertices = _vertices.ToArray(),
            triangles = _triangles.ToArray(),
            uv = _uvs.ToArray()
        };
        mesh.RecalculateNormals();

        _meshFilter.mesh = mesh;
    }

    private void ClearMeshData()
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();
    }

    private void UpdateMeshData(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + VoxelData.FaceChecks[p]))
            {
                byte blockId = VoxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 3]]);

                AddTexture(_world.BlockTypes[blockId].GetTextureId(p));

                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
                _vertexIndex += 4;

            }
        }
    }

    private void AddTexture(int textureId)
    {
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        _uvs.Add(new Vector2(x, y));
        _uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        _uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        _uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}
