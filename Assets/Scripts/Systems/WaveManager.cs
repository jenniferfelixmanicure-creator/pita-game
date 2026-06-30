using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gerencia ondas de inimigos com dificuldade progressiva.
/// Suporta inimigos normais, elites e chefes.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [System.Serializable]
    public class EnemySpawnEntry
    {
        public GameObject prefab;
        public int weight = 10;
        public float minWaveToAppear = 1f;
        public bool isBoss = false;
        public bool isElite = false;
    }

    [System.Serializable]
    public class WaveConfig
    {
        public float startTime;
        public int baseCount;
        public float spawnInterval;
        public bool spawnBoss;
    }

    [Header("Configuração de Ondas")]
    [SerializeField] private List<EnemySpawnEntry> enemyPool;
    [SerializeField] private List<WaveConfig> waveConfigs;
    [SerializeField] private float spawnRadius = 12f;
    [SerializeField] private float difficultyScalePerMinute = 0.1f;

    [Header("Chefes")]
    [SerializeField] private float firstBossAt = 120f;
    [SerializeField] private float bossInterval = 120f;

    private float gameTime;
    private float difficultyMultiplier = 1f;
    private int activeEnemies = 0;
    private int totalEnemiesSpawned = 0;
    private float nextBossTime;
    private Coroutine spawnCoroutine;

    public int ActiveEnemies => activeEnemies;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        nextBossTime = firstBossAt;
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;
        gameTime += Time.deltaTime;
        difficultyMultiplier = 1f + (gameTime / 60f) * difficultyScalePerMinute;

        // Boss timer
        if (gameTime >= nextBossTime)
        {
            nextBossTime += bossInterval;
            StartCoroutine(SpawnBoss());
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => GameManager.Instance.IsPlaying);
            yield return StartCoroutine(SpawnWave());
        }
    }

    private IEnumerator SpawnWave()
    {
        int count = Mathf.RoundToInt(5 + (gameTime / 30f) * 3f);
        count = Mathf.Min(count, 50);

        float interval = Mathf.Max(0.1f, 0.5f - (gameTime / 600f) * 0.3f);

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(interval);
        }

        // Pausa entre ondas diminui com o tempo
        float pause = Mathf.Max(1f, 5f - (gameTime / 120f));
        yield return new WaitForSeconds(pause);
    }

    private void SpawnEnemy()
    {
        var prefab = GetWeightedEnemy();
        if (prefab == null) return;

        Vector2 spawnPos = GetSpawnPosition();
        var obj = EnemyPool.Instance != null
            ? EnemyPool.Instance.GetEnemy(prefab)
            : Instantiate(prefab, spawnPos, Quaternion.identity);

        if (obj == null) return;
        obj.transform.position = spawnPos;

        var enemy = obj.GetComponent<EnemyBase>();
        enemy?.ApplyDifficultyScale(difficultyMultiplier);

        activeEnemies++;
        totalEnemiesSpawned++;
    }

    private IEnumerator SpawnBoss()
    {
        AudioManager.Instance?.Play("boss_incoming");
        UIManager.Instance?.ShowBossWarning();
        yield return new WaitForSeconds(3f);

        var bossPrefab = enemyPool.Find(e => e.isBoss)?.prefab;
        if (bossPrefab == null) yield break;

        Vector2 pos = GetSpawnPosition();
        var obj = Instantiate(bossPrefab, pos, Quaternion.identity);
        var boss = obj.GetComponent<BossBase>();
        boss?.ApplyDifficultyScale(difficultyMultiplier * 1.5f);

        activeEnemies++;
        AudioManager.Instance?.PlayMusic("boss_music");
    }

    public void OnEnemyDied()
    {
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
    }

    private GameObject GetWeightedEnemy()
    {
        var available = enemyPool.FindAll(e =>
            !e.isBoss && gameTime >= e.minWaveToAppear * 60f);

        if (available.Count == 0) return enemyPool[0]?.prefab;

        int totalWeight = 0;
        available.ForEach(e => totalWeight += e.weight);

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in available)
        {
            cumulative += entry.weight;
            if (roll < cumulative) return entry.prefab;
        }
        return available[0].prefab;
    }

    private Vector2 GetSpawnPosition()
    {
        if (PlayerController.Instance == null) return Vector2.zero;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
        return (Vector2)PlayerController.Instance.transform.position + offset;
    }

    public float GetDifficulty() => difficultyMultiplier;
    public float GetGameTime() => gameTime;
}
