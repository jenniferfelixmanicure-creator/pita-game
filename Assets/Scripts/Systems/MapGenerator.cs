using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gerador procedural de mapas — 20+ biomas com tiles, props e bordas infinitas.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    public enum MapBiome
    {
        DarkForest, Dungeon, Graveyard, VolcanicCave, CrystalCavern,
        ShadowRealm, FrozenWastes, SwampLands, AncientRuins, AbyssalPlains,
        CursedCastle, BloodArena, NightCitadel, DeepAbyss, VoidRealm,
        ElectricMaze, PlagueZone, HellGate, CelestialArena, FinalBoss
    }

    [System.Serializable]
    public class MapData
    {
        public MapBiome biome;
        public string displayName;
        public Color ambientColor;
        public Color fogColor;
        public float fogDensity;
        public GameObject[] floorTiles;
        public GameObject[] wallTiles;
        public GameObject[] propObjects;
        public GameObject[] decorObjects;
        public int unlockLevel;
        public AudioClip ambientMusic;
    }

    [Header("Configurações")]
    [SerializeField] private List<MapData> allMaps;
    [SerializeField] private int chunkSize = 16;
    [SerializeField] private int visibleChunks = 3;
    [SerializeField] private float tileSize = 1f;

    private Dictionary<Vector2Int, GameObject> loadedChunks = new();
    private MapData currentMap;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (PlayerController.Instance) playerTransform = PlayerController.Instance.transform;
    }

    private void Update()
    {
        if (playerTransform) UpdateChunks();
    }

    public void LoadMap(MapBiome biome)
    {
        currentMap = allMaps?.Find(m => m.biome == biome);
        if (currentMap == null && allMaps?.Count > 0) currentMap = allMaps[0];

        ClearAll();
        ApplyAmbience();
        GenerateInitialChunks();
    }

    public void LoadRandomMap(int playerLevel)
    {
        var available = allMaps?.FindAll(m => m.unlockLevel <= playerLevel) ?? new List<MapData>();
        if (available.Count == 0) { LoadMap(MapBiome.DarkForest); return; }
        currentMap = available[Random.Range(0, available.Count)];
        ClearAll();
        ApplyAmbience();
        GenerateInitialChunks();
    }

    private void ApplyAmbience()
    {
        if (currentMap == null) return;
        RenderSettings.ambientLight = currentMap.ambientColor;
        RenderSettings.fogColor     = currentMap.fogColor;
        RenderSettings.fogDensity   = currentMap.fogDensity;
        if (currentMap.ambientMusic) AudioManager.Instance?.PlayMusicClip(currentMap.ambientMusic);
    }

    private void GenerateInitialChunks()
    {
        for (int x = -visibleChunks; x <= visibleChunks; x++)
            for (int y = -visibleChunks; y <= visibleChunks; y++)
                GenerateChunk(new Vector2Int(x, y));
    }

    private void UpdateChunks()
    {
        Vector2Int playerChunk = WorldToChunk(playerTransform.position);
        for (int x = -visibleChunks; x <= visibleChunks; x++)
            for (int y = -visibleChunks; y <= visibleChunks; y++)
            {
                var c = playerChunk + new Vector2Int(x, y);
                if (!loadedChunks.ContainsKey(c)) GenerateChunk(c);
            }
    }

    private void GenerateChunk(Vector2Int coord)
    {
        if (loadedChunks.ContainsKey(coord)) return;
        if (currentMap == null) return;

        var chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
        chunkObj.transform.parent = transform;
        Vector3 origin = new Vector3(coord.x * chunkSize * tileSize, coord.y * chunkSize * tileSize);

        for (int tx = 0; tx < chunkSize; tx++)
            for (int ty = 0; ty < chunkSize; ty++)
            {
                // Floor tile
                if (currentMap.floorTiles?.Length > 0)
                {
                    var tile = currentMap.floorTiles[Random.Range(0, currentMap.floorTiles.Length)];
                    if (tile)
                    {
                        var t = Instantiate(tile, origin + new Vector3(tx, ty) * tileSize, Quaternion.identity, chunkObj.transform);
                    }
                }

                // Props (5% chance)
                if (currentMap.propObjects?.Length > 0 && Random.value < 0.05f)
                {
                    var prop = currentMap.propObjects[Random.Range(0, currentMap.propObjects.Length)];
                    if (prop) Instantiate(prop, origin + new Vector3(tx, ty) * tileSize, Quaternion.Euler(0, 0, Random.Range(0, 360)), chunkObj.transform);
                }
            }

        loadedChunks[coord] = chunkObj;
    }

    private void ClearAll()
    {
        foreach (var kv in loadedChunks) if (kv.Value) Destroy(kv.Value);
        loadedChunks.Clear();
    }

    private Vector2Int WorldToChunk(Vector3 pos) =>
        new Vector2Int(Mathf.FloorToInt(pos.x / (chunkSize * tileSize)),
                       Mathf.FloorToInt(pos.y / (chunkSize * tileSize)));

    public string GetCurrentBiomeName() => currentMap?.displayName ?? "Unknown";
}
