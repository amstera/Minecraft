using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int Seed;
    public BiomeAttribute Biome;
    public Transform Player;
    public Vector3 SpawnPosition;

    public Material Material;
    public BlockType[] BlockTypes;

    Chunk[,] Chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> ActiveChunks = new List<ChunkCoord>();
    ChunkCoord PlayerLastChunkCoord;
    List<ChunkCoord> ChunksToCreate = new List<ChunkCoord>();
    List<Chunk> ChunksToUpdate = new List<Chunk>();
    Queue<VoxelMod> Modifications = new Queue<VoxelMod>();

    private bool _applyingModifications;

    private void Start()
    {
        Random.InitState(Seed);

        GenerateWorld();
        PlayerLastChunkCoord = GetChunkCoordFromVector3(Player.transform.position);
    }

    private void Update()
    {
        if (!GetChunkCoordFromVector3(Player.transform.position).Equals(PlayerLastChunkCoord))
        {
            CheckViewDistance();
        }

        if (Modifications.Count > 0 && !_applyingModifications)
        {
            StartCoroutine(ApplyModifications());
        }

        if (ChunksToCreate.Count > 0)
        {
            CreateChunk();
        }

        if (ChunksToUpdate.Count > 0)
        {
            UpdateChunks();
        }
    }

    public Chunk GetChunkFromVector3 (Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return Chunks[x, z];
    } 

    private ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    private void CreateChunk()
    {
        ChunkCoord c = ChunksToCreate[0];
        ChunksToCreate.RemoveAt(0);
        ActiveChunks.Add(c);
        Chunks[c.x, c.z].Init();
    }

    private void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        while (!updated && index < ChunksToUpdate.Count - 1)
        {
            if (ChunksToUpdate[index].IsVoxelMapPopulated)
            {
                ChunksToUpdate[index].UpdateChunk();
                ChunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
            {
                index++;
            }
        }
    }

    private IEnumerator ApplyModifications()
    {
        _applyingModifications = true;
        int count = 0;

        while (Modifications.Count > 0)
        {
            VoxelMod v = Modifications.Dequeue();
            ChunkCoord c = GetChunkCoordFromVector3(v.Position);
            if (Chunks[c.x, c.z] == null)
            {
                Chunks[c.x, c.z] = new Chunk(c, this, true);
                ActiveChunks.Add(c);
            }

            Chunks[c.x, c.z].Modifications.Enqueue(v);

            if (!ChunksToUpdate.Contains(Chunks[c.x, c.z]))
            {
                ChunksToUpdate.Add(Chunks[c.x, c.z]);
            }

            count++;
            if (count > 200)
            {
                count = 0;
                yield return null;
            }
        }

        _applyingModifications = false;
    }

    private void GenerateWorld()
    {
        for (int x = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks / 2; x < VoxelData.WorldSizeInChunks / 2 + VoxelData.ViewDistanceInChunks / 2; x++)
        {
            for (int z = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks / 2; z < VoxelData.WorldSizeInChunks / 2 + VoxelData.ViewDistanceInChunks / 2; z++)
            {
                Chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                ActiveChunks.Add(new ChunkCoord(x, z));
            }
        }

        while (Modifications.Count > 0)
        {
            VoxelMod v = Modifications.Dequeue();

            ChunkCoord c = GetChunkCoordFromVector3(v.Position);
            if (Chunks[c.x, c.z] == null)
            {
                Chunks[c.x, c.z] = new Chunk(c, this, true);
                ActiveChunks.Add(c);
            }

            Chunks[c.x, c.z].Modifications.Enqueue(v);

            if (!ChunksToUpdate.Contains(Chunks[c.x, c.z]))
            {
                ChunksToUpdate.Add(Chunks[c.x, c.z]);
            }
        }

        for (int i = 0; i < ChunksToUpdate.Count; i++)
        {
            ChunksToUpdate[0].UpdateChunk();
            ChunksToUpdate.RemoveAt(0);
        }

        SpawnPosition = new Vector3(VoxelData.WorldSizeInBlocks / 2, VoxelData.ChunkHeight - 50, VoxelData.WorldSizeInBlocks / 2);
        Player.position = SpawnPosition;
    }

    private void CheckViewDistance()
    {
        int chunkX = Mathf.FloorToInt(Player.position.x / VoxelData.ChunkWidth);
        int chunkZ = Mathf.FloorToInt(Player.position.z / VoxelData.ChunkWidth);

        PlayerLastChunkCoord = GetChunkCoordFromVector3(Player.transform.position);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(ActiveChunks);

        for (int x = chunkX - VoxelData.ViewDistanceInChunks / 2; x < chunkX + VoxelData.ViewDistanceInChunks / 2; x++)
        {
            for (int z = chunkZ - VoxelData.ViewDistanceInChunks / 2; z < chunkZ + VoxelData.ViewDistanceInChunks / 2; z++)
            {
                // If the chunk is within the world bounds and it has not been created.
                if (IsChunkInWorld(x, z))
                {
                    ChunkCoord thisChunk = new ChunkCoord(x, z);

                    if (Chunks[x, z] == null)
                    {
                        Chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        ChunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!Chunks[x, z].IsActive)
                    {
                        Chunks[x, z].IsActive = true;
                        ActiveChunks.Add(thisChunk);
                    }
                    // Check if this chunk was already in the active chunks list.
                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {

                        //if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                    }

                }
            }
        }

        foreach (ChunkCoord coord in previouslyActiveChunks)
        {
            Chunks[coord.x, coord.z].IsActive = false;
        }

    }

    private bool IsChunkInWorld(int x, int z)
    {
        return x > 0 && x < VoxelData.WorldSizeInChunks - 1 && z > 0 && z < VoxelData.WorldSizeInChunks - 1;
    }

    private bool IsVoxelInWorld(Vector3 pos)
    {
        return pos.x < 0 || pos.x > VoxelData.WorldSizeInBlocks - 1 || pos.y < 0 || pos.y > VoxelData.ChunkHeight - 1 || pos.z < 0 || pos.z > VoxelData.WorldSizeInBlocks - 1;
    }

    public bool CheckForVoxel(float x, float y, float z)
    {
        return CheckForVoxel(new Vector3(x, y, z));
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord chunkCoord = new ChunkCoord(pos);
        if (IsVoxelInWorld(pos))
        {
            return false;
        }
        if (Chunks[chunkCoord.x, chunkCoord.z] != null && Chunks[chunkCoord.x, chunkCoord.z].IsVoxelMapPopulated)
        {
            return BlockTypes[Chunks[chunkCoord.x, chunkCoord.z].GetVoxelFromGlobalVector3(pos)].IsSolid;
        }

        return BlockTypes[GetVoxel(pos)].IsSolid;
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        if (IsVoxelInWorld(pos))
        {
            return (byte)Blocks.Empty;
        }

        if (yPos == 0)
        {
            return (byte)Blocks.Bedrock;
        }

        int terrainHeight = Mathf.FloorToInt(Biome.TerrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, Biome.TerrainScale)) + Biome.SolidGroundHeight;
        Blocks voxelValue;

        //first pass

        if (yPos == terrainHeight)
        {
            voxelValue = Blocks.Grass;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = Blocks.Dirt;
        }
        else if (yPos > terrainHeight)
        {
            return (byte)Blocks.Empty;
        }
        else
        {
            voxelValue = Blocks.Stone;
        }

        // second pass

        if (voxelValue == Blocks.Stone)
        {
            foreach (Lode lode in Biome.Lodes)
            {
                if (yPos > lode.MinHeight && yPos > lode.MaxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.NoiseOffset, lode.Scale, lode.Threshold))
                    {
                        voxelValue = lode.Block;
                    }
                }
            }
        }

        // tree pass

        if (yPos == terrainHeight)
        {
            if (Noise.Get2DPerlin(new Vector3(pos.x, pos.z), 0, Biome.TreeZoneScale) > Biome.TreeZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector3(pos.x, pos.z), 0, Biome.TreePlacementScale) > Biome.TreePlacementThreshold)
                {
                    Structure.MakeTree(pos, Modifications, Biome.MinTreeHeight, Biome.MaxTreeHeight);
                }
            }
        }

        return (byte)voxelValue;
    }

}

[System.Serializable]
public class BlockType
{

    public string blockName;
    public bool IsSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureId(int faceIndex)
    {
        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                return 0;

        }

    }

}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
        {
            return false;
        }
        if (other.x == x && other.z == z)
        {
            return true;
        }
        return false;
    }
}

public class VoxelMod
{
    public Vector3 Position;
    public Blocks BlockId;

    public VoxelMod()
    {
        Position = new Vector3();
        BlockId = Blocks.Empty;
    }

    public VoxelMod(Vector3 pos, Blocks id)
    {
        Position = pos;
        BlockId = id;
    }
}

public enum Blocks
{
    Empty = 0,
    Bedrock = 1,
    Stone = 2,
    Grass = 3,
    Dirt = 4,
    Wood = 5,
    Leaves = 6
}