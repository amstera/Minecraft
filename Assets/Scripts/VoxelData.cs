using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunks = 125;
    public static int WorldSizeInBlocks
    {

        get { return WorldSizeInChunks * ChunkWidth; }

    }

    public static readonly int ViewDistanceInChunks = 12;

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] VoxelVerts = {
        Vector3.zero,
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        Vector3.one,
        new Vector3(0, 1, 1)
    };

    public static readonly Vector3[] FaceChecks =
    {
        new Vector3(0, 0, -1),
        new Vector3(0, 0, 1),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(-1, 0, 0),
        new Vector3(1, 0, 0)
    };

    public static readonly int[,] VoxelTris =
    {
        {0, 3, 1, 2 }, // Back face
        {5, 6, 4, 7 }, // Font face
        {3, 7, 2, 6 }, // Top face
        {1, 5, 0, 4 }, // Bottom face
        {4, 7, 0, 3 }, // Left face
        {1, 2, 5, 6} // Right face
    };

    public static readonly Vector2[] VoxelUvs =
    {
        Vector2.zero,
        new Vector2(0, 1),
        new Vector2(1, 0),
        Vector2.one
    };
}
