using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public int Seed;
    public BiomeAttribute Biome;
    public Transform Player;
    public float MobSpawnTimeSeconds = 10;

    public Material Material;
    public BlockType[] BlockTypes;

    public GameObject DirtBlock;
    public GameObject WoodBlock;
    public GameObject StoneBlock;
    public GameObject DiamondBlock;
    public GameObject GoldBlock;
    public GameObject BlockParticles;

    public GameObject Status;

    public AudioSource MusicAudioSource;
    public Toggle Music;

    public Light Light;
    public Material Skybox;
    public Color[] SkyDayColors = new Color[2];
    public Color[] SkyNightColors = new Color[2];

    public List<GameObject> Mobs;

    private Camera _cam;
    private Chunk[,] _chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    private List<ChunkCoord> _activeChunks = new List<ChunkCoord>();
    private ChunkCoord _playerLastChunkCoord;
    private List<ChunkCoord> _chunksToCreate = new List<ChunkCoord>();
    private List<Chunk> _chunksToUpdate = new List<Chunk>();
    private Queue<VoxelMod> _modifications = new Queue<VoxelMod>();

    private float _timePassed;
    private bool _isNight;
    private float _frozenTime = -10;

    private bool _applyingModifications;

    private void Start()
    {
        Seed = Menu.Instance.Seed;
        if (!Menu.Instance.MusicOn)
        {
            Music.isOn = false;
        }
        Random.InitState(Seed);
        _cam = Camera.main;
        GenerateWorld();
        if (MobSpawnTimeSeconds > 0 && !Menu.Instance.NoMobs)
        {
            StartCoroutine(SpawnMobs());
        }
        _playerLastChunkCoord = GetChunkCoordFromVector3(Player.transform.position);
    }

    private void Update()
    {
        if (!GetChunkCoordFromVector3(Player.transform.position).Equals(_playerLastChunkCoord))
        {
            CheckViewDistance();
        }

        if (_modifications.Count > 0 && !_applyingModifications)
        {
            StartCoroutine(ApplyModifications());
        }

        if (_chunksToCreate.Count > 0)
        {
            CreateChunk();
        }

        if (_chunksToUpdate.Count > 0)
        {
            UpdateChunks();
        }

        UpdateStatus();

        UpdateSky();
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return _chunks[x, z];
    }

    private ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public void GenerateBlock(Blocks block, Vector3 pos)
    {
        GameObject selectedBlock = null;
        if (block == Blocks.Dirt || block == Blocks.Grass)
        {
            selectedBlock = DirtBlock;
        }
        else if (block == Blocks.Wood)
        {
            selectedBlock = WoodBlock;
        }
        else if (block == Blocks.Stone)
        {
            selectedBlock = StoneBlock;
        }
        else if (block == Blocks.Diamond)
        {
            selectedBlock = DiamondBlock;
        }
        else if (block == Blocks.Gold)
        {
            selectedBlock = GoldBlock;
        }
        if (selectedBlock != null)
        {
            Vector3 updatedPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f);
            GameObject particles = Instantiate(BlockParticles, updatedPos, Quaternion.identity);
            Destroy(particles, 2);
            Instantiate(selectedBlock, updatedPos, Quaternion.identity);
        }
    }

    private void CreateChunk()
    {
        ChunkCoord c = _chunksToCreate[0];
        _chunksToCreate.RemoveAt(0);
        _activeChunks.Add(c);
        _chunks[c.x, c.z].Init();
    }

    private void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        while (!updated && index < _chunksToUpdate.Count - 1)
        {
            if (_chunksToUpdate[index].IsVoxelMapPopulated)
            {
                _chunksToUpdate[index].UpdateChunk();
                _chunksToUpdate.RemoveAt(index);
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

        while (_modifications.Count > 0)
        {
            VoxelMod v = _modifications.Dequeue();
            ChunkCoord c = GetChunkCoordFromVector3(v.Position);
            if (_chunks[c.x, c.z] == null)
            {
                _chunks[c.x, c.z] = new Chunk(c, this, true);
                _activeChunks.Add(c);
            }

            _chunks[c.x, c.z].Modifications.Enqueue(v);

            if (!_chunksToUpdate.Contains(_chunks[c.x, c.z]))
            {
                _chunksToUpdate.Add(_chunks[c.x, c.z]);
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
                _chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                _activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        while (_modifications.Count > 0)
        {
            VoxelMod v = _modifications.Dequeue();

            ChunkCoord c = GetChunkCoordFromVector3(v.Position);
            if (_chunks[c.x, c.z] == null)
            {
                _chunks[c.x, c.z] = new Chunk(c, this, true);
                _activeChunks.Add(c);
            }

            _chunks[c.x, c.z].Modifications.Enqueue(v);

            if (!_chunksToUpdate.Contains(_chunks[c.x, c.z]))
            {
                _chunksToUpdate.Add(_chunks[c.x, c.z]);
            }
        }

        for (int i = 0; i < _chunksToUpdate.Count; i++)
        {
            _chunksToUpdate[0].UpdateChunk();
            _chunksToUpdate.RemoveAt(0);
        }

        Player.position = new Vector3(VoxelData.WorldSizeInBlocks / 2, VoxelData.ChunkHeight - 60, VoxelData.WorldSizeInBlocks / 2);
    }

    private void CheckViewDistance()
    {
        int chunkX = Mathf.FloorToInt(Player.position.x / VoxelData.ChunkWidth);
        int chunkZ = Mathf.FloorToInt(Player.position.z / VoxelData.ChunkWidth);

        _playerLastChunkCoord = GetChunkCoordFromVector3(Player.transform.position);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(_activeChunks);

        for (int x = chunkX - VoxelData.ViewDistanceInChunks / 2; x < chunkX + VoxelData.ViewDistanceInChunks / 2; x++)
        {
            for (int z = chunkZ - VoxelData.ViewDistanceInChunks / 2; z < chunkZ + VoxelData.ViewDistanceInChunks / 2; z++)
            {
                // If the chunk is within the world bounds and it has not been created.
                if (IsChunkInWorld(x, z))
                {
                    ChunkCoord thisChunk = new ChunkCoord(x, z);

                    if (_chunks[x, z] == null)
                    {
                        _chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        _chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!_chunks[x, z].IsActive)
                    {
                        _chunks[x, z].IsActive = true;
                        _activeChunks.Add(thisChunk);
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
            _chunks[coord.x, coord.z].IsActive = false;
        }

    }

    private bool IsChunkInWorld(int x, int z)
    {
        if (x > 0 && x < VoxelData.WorldSizeInChunks - 1 && z > 0 && z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }

        return false;
    }

    private bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInBlocks && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInBlocks)
        {
            return true;
        }

        return false;
    }

    public bool CheckForVoxel(float x, float y, float z)
    {
        return CheckForVoxel(new Vector3(x, y, z));
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord chunkCoord = new ChunkCoord(pos);
        if (!IsChunkInWorld(chunkCoord.x, chunkCoord.z) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return false;
        }
        if (_chunks[chunkCoord.x, chunkCoord.z] != null && _chunks[chunkCoord.x, chunkCoord.z].IsVoxelMapPopulated)
        {
            return BlockTypes[_chunks[chunkCoord.x, chunkCoord.z].GetVoxelFromGlobalVector3(pos)].IsSolid;
        }

        return BlockTypes[GetVoxel(pos)].IsSolid;
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        if (!IsVoxelInWorld(pos))
        {
            return (byte)Blocks.Empty;
        }

        if (yPos == 0)
        {
            return (byte)Blocks.Bedrock;
        }

        int terrainHeight = Mathf.FloorToInt(Biome.TerrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), Seed, Biome.TerrainScale)) + Biome.SolidGroundHeight;
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
                if (yPos > lode.MinHeight && yPos < lode.MaxHeight)
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
            if (Noise.Get2DPerlin(new Vector3(pos.x, pos.z), Seed, Biome.TreeZoneScale) > Biome.TreeZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector3(pos.x, pos.z), Seed, Biome.TreePlacementScale) > Biome.TreePlacementThreshold)
                {
                    Structure.MakeTree(pos, _modifications, Biome.MinTreeHeight, Biome.MaxTreeHeight);
                }
            }
        }

        return (byte)voxelValue;
    }

    public void ResetWorld()
    {
        SceneManager.LoadScene("Game");
    }

    public void ChangeMusic(bool isOn)
    {
        Menu.Instance.MusicOn = isOn;
        if (isOn)
        {
            MusicAudioSource.Play();
        }
        else
        {
            MusicAudioSource.Stop();
        }
    }

    public void FreezeTime()
    {
        _frozenTime = Time.time;
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.Freeze();
        }
        StartCoroutine(UnfreezeTime());
    }

    private IEnumerator UnfreezeTime()
    {
        yield return new WaitForSeconds(10);

        if (Time.time - _frozenTime >= 10)
        {
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                enemy.Unfreeze();
            }
        }
    }

    private IEnumerator SpawnMobs()
    {
        yield return new WaitForSeconds(MobSpawnTimeSeconds);

        List<GameObject> possibleMobs = new List<GameObject>
        {
            Mobs[0]
        };
        if (FindObjectsOfType<Enemy>().Count(e => e.Type == EnemyType.Creeper) < 2)
        {
            possibleMobs.Add(Mobs[1]);
        }
        if (FindObjectsOfType<Enemy>().Count(e => e.Type == EnemyType.FlyingCreeper) == 0)
        {
            possibleMobs.Add(Mobs[2]);
        }
        GameObject mob = possibleMobs[Random.Range(0, possibleMobs.Count)];
        Vector3 pos;
        if (mob.GetComponent<Enemy>().Type == EnemyType.FlyingCreeper)
        {
            pos = new Vector3(Player.transform.position.x - (_cam.transform.forward.x * 10f), VoxelData.ChunkHeight - 55, Player.transform.position.z - (_cam.transform.forward.z * 10f));
        }
        else
        {
            pos = new Vector3(Player.transform.position.x - (_cam.transform.forward.x * 10f), Mathf.Max(VoxelData.ChunkHeight - 75, Player.transform.position.y), Player.transform.position.z - (_cam.transform.forward.z * 10f));
        }
        if (_timePassed >= 50)
        {
            if (Time.time - _frozenTime >= 10)
            {
                Instantiate(mob, pos, Quaternion.identity);
            }
        }
        else
        {
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                enemy.TakeDamage(enemy.gameObject.transform.forward, null, enemy.Health, 8);
            }
        }

        if (!Player.GetComponent<Player>().IsDead)
        {
            StartCoroutine(SpawnMobs());
        }
    }

    private void UpdateStatus()
    {
        if (Time.time - _frozenTime < 10)
        {
            Status.SetActive(true);
            Text text = Status.transform.Find("Time").GetComponent<Text>();
            int timeRemaining = Mathf.FloorToInt(10 - (Time.time - _frozenTime)) + 1;
            text.text = $"00:{timeRemaining.ToString("00")}";
        }
        else
        {
            Status.SetActive(false);
        }
    }

    private void UpdateSky()
    {
        if (_isNight)
        {
            _timePassed -= Time.deltaTime;
            if (_timePassed <= 0)
            {
                _isNight = false;
            }
        }
        else
        {
            _timePassed += Time.deltaTime;
            if (_timePassed >= 90)
            {
                _isNight = true;
            }
        }

        Light.intensity = 1 - _timePassed / 90;
        Skybox.SetColor("_SkyGradientTop", Color.Lerp(SkyDayColors[0], SkyNightColors[0], _timePassed / 90));
        Skybox.SetColor("_SkyGradientBottom", Color.Lerp(SkyDayColors[1], SkyNightColors[1], _timePassed / 90));
        Skybox.SetFloat("_SunHaloContribution", (90 -_timePassed) / 90 * 0.2f);
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
    Stopwatch = -3,
    Gun = -2,
    Sword = -1,
    Empty = 0,
    Bedrock = 1,
    Stone = 2,
    Grass = 3,
    Dirt = 4,
    Wood = 5,
    Leaves = 6,
    Diamond = 7,
    Gold = 8,
    GoldBlock = 9
}