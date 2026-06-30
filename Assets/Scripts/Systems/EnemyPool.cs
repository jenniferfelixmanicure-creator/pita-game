using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object Pool para inimigos — evita Instantiate/Destroy constante.
/// Mantém 60 FPS mesmo com centenas de inimigos na tela.
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [System.Serializable]
    public class PoolEntry
    {
        public GameObject prefab;
        public int initialSize = 20;
    }

    [SerializeField] private List<PoolEntry> poolEntries;

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        foreach (var entry in poolEntries)
        {
            string key = entry.prefab.name;
            prefabMap[key] = entry.prefab;
            pools[key] = new Queue<GameObject>();

            for (int i = 0; i < entry.initialSize; i++)
            {
                var obj = Instantiate(entry.prefab, transform);
                obj.SetActive(false);
                pools[key].Enqueue(obj);
            }
        }
    }

    public GameObject GetEnemy(GameObject prefab)
    {
        string key = prefab.name;
        if (!pools.ContainsKey(key))
        {
            prefabMap[key] = prefab;
            pools[key] = new Queue<GameObject>();
        }

        GameObject obj;
        if (pools[key].Count > 0)
        {
            obj = pools[key].Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, transform);
        }

        obj.SetActive(true);
        return obj;
    }

    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        string key = enemy.name.Replace("(Clone)", "").Trim();
        if (pools.ContainsKey(key))
            pools[key].Enqueue(enemy);
        else
            Destroy(enemy);

        WaveManager.Instance?.OnEnemyDied();
    }
}
