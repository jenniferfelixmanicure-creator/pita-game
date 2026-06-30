using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>Pool de números de dano flutuantes — performance máxima.</summary>
public class DamageNumberPool : MonoBehaviour
{
    public static DamageNumberPool Instance { get; private set; }

    [SerializeField] private GameObject normalPrefab;
    [SerializeField] private GameObject critPrefab;
    [SerializeField] private int poolSize = 50;

    private Queue<GameObject> normalPool = new Queue<GameObject>();
    private Queue<GameObject> critPool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        PreWarm(normalPrefab, normalPool, poolSize);
        PreWarm(critPrefab, critPool, poolSize / 2);
    }

    private void PreWarm(GameObject prefab, Queue<GameObject> pool, int count)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++)
        {
            var o = Instantiate(prefab, transform);
            o.SetActive(false);
            pool.Enqueue(o);
        }
    }

    public void ShowDamage(Vector3 pos, float damage)
    {
        Show(normalPool, normalPrefab, pos, damage, Color.white);
    }

    public void ShowCrit(Vector3 pos, float damage)
    {
        Show(critPool, critPrefab, pos, damage, Color.yellow);
    }

    public void ShowHeal(Vector3 pos, float amount)
    {
        Show(normalPool, normalPrefab, pos, amount, Color.green);
    }

    private void Show(Queue<GameObject> pool, GameObject prefab, Vector3 pos, float value, Color color)
    {
        if (prefab == null) return;
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
        obj.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.3f;
        obj.SetActive(true);

        var tmp = obj.GetComponent<TextMeshPro>();
        if (tmp) { tmp.text = Mathf.RoundToInt(value).ToString(); tmp.color = color; }

        StartCoroutine(AnimateAndReturn(obj, pool));
    }

    private IEnumerator AnimateAndReturn(GameObject obj, Queue<GameObject> pool)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;
        var tmp = obj.GetComponent<TextMeshPro>();
        Color startColor = tmp ? tmp.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.position = startPos + Vector3.up * (t * 1.5f);
            if (tmp) tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }

        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
