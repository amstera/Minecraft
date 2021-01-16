using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static void MakeTree(Vector3 position, Queue<VoxelMod> voxelMods, int minTrunkHeight, int maxTrunkHeight)
    {
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector3(position.x, position.z), 250, 3));

        if (height < minTrunkHeight)
        {
            height = minTrunkHeight;
        }

        for (int i = 1; i < height; i++)
        {
            voxelMods.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), Blocks.Wood));
        }

        for (int y = 0; y < 3; y++)
        {
            int xWidth = Random.Range(2, 4);
            int zWidth = Random.Range(2, 4);

            for (int z = 0 - zWidth/2; z < zWidth; z++)
            {
                for (int x = 0 - xWidth / 2; x < xWidth; x++)
                {
                    voxelMods.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + y + height, position.z + z), Blocks.Leaves));
                }
            }
        }
    }
}
