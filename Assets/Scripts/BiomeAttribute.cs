using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttribute : ScriptableObject
{
    public string BiomeName;

    public int SolidGroundHeight;
    public int TerrainHeight;
    public float TerrainScale;

    [Header("Trees")]
    public float TreeZoneScale = 1.3f;
    [Range(0.1f, 1f)]
    public float TreeZoneThreshold = 0.6f;
    public float TreePlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float TreePlacementThreshold = 0.8f;

    public int MaxTreeHeight = 12;
    public int MinTreeHeight = 5;

    public Lode[] Lodes;
}

[System.Serializable]
public class Lode
{
    public string NodeName;
    public Blocks Block;
    public int MinHeight;
    public int MaxHeight;
    public float Scale;
    public float Threshold;
    public float NoiseOffset;
}
