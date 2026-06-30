using UnityEngine;
using System.Collections.Generic;

/// <summary>Pool de projéteis — reutiliza objetos para performance máxima.</summary>
public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [System.Serializable]
    public class PoolEntry { public GameObject prefab; public int initialSize = 30; }
    [SerializeField] private List<PoolEntry> entries;

    private Dictionary<string, Queue<GameObject>> pools = new();
    private Dictionary<string, GameObject> prefabs = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        foreach (var e in entries)
        {
            string k = e.prefab.name;
            prefabs[k] = e.prefab;
            pools[k] = new Queue<GameObject>();
            for (int i = 0; i < e.initialSize; i++)
            {
                var o = Instantiate(e.prefab, transform);
                o.SetActive(false);
                pools[k].Enqueue(o);
            }
        }
    }

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        string k = prefab.name;
        if (!pools.ContainsKey(k)) { prefabs[k] = prefab; pools[k] = new Queue<GameObject>(); }
        GameObject obj = pools[k].Count > 0 ? pools[k].Dequeue() : Instantiate(prefab, transform);
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject proj)
    {
        proj.SetActive(false);
        string k = proj.name.Replace("(Clone)", "").Trim();
        if (pools.ContainsKey(k)) pools[k].Enqueue(proj);
        else Destroy(proj);
    }
}
