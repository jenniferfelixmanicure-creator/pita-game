using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>Minimapa com pontos de inimigos, jogador e chefes.</summary>
public class MinimapSystem : MonoBehaviour
{
    public static MinimapSystem Instance { get; private set; }

    [Header("Referências")]
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private GameObject playerDot;
    [SerializeField] private GameObject enemyDotPrefab;
    [SerializeField] private GameObject bossDotPrefab;
    [SerializeField] private GameObject chestDotPrefab;

    [Header("Config")]
    [SerializeField] private float worldSize = 50f;
    [SerializeField] private Color playerColor  = Color.cyan;
    [SerializeField] private Color enemyColor   = Color.red;
    [SerializeField] private Color bossColor    = new Color(1f, 0.3f, 0f);
    [SerializeField] private Color chestColor   = Color.yellow;

    private List<(Transform world, RectTransform dot)> trackedEnemies = new();
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

    private void LateUpdate()
    {
        if (!mapRect) return;

        // Move ponto do jogador
        if (playerTransform && playerDot)
            SetDotPos(playerDot.GetComponent<RectTransform>(), playerTransform.position);

        // Remove dots de inimigos mortos
        trackedEnemies.RemoveAll(pair =>
        {
            if (pair.world == null) { Destroy(pair.dot.gameObject); return true; }
            SetDotPos(pair.dot, pair.world.position);
            return false;
        });
    }

    public void TrackEnemy(Transform enemy, bool isBoss)
    {
        if (!mapRect) return;
        var prefab = isBoss ? bossDotPrefab : enemyDotPrefab;
        if (!prefab) return;
        var dot = Instantiate(prefab, mapRect).GetComponent<RectTransform>();
        dot.GetComponent<Image>().color = isBoss ? bossColor : enemyColor;
        trackedEnemies.Add((enemy, dot));
    }

    public void TrackChest(Transform chest)
    {
        if (!mapRect || !chestDotPrefab) return;
        var dot = Instantiate(chestDotPrefab, mapRect).GetComponent<RectTransform>();
        dot.GetComponent<Image>().color = chestColor;
        trackedEnemies.Add((chest, dot));
    }

    private void SetDotPos(RectTransform dot, Vector3 worldPos)
    {
        float halfMap = worldSize * 0.5f;
        float nx = Mathf.InverseLerp(-halfMap, halfMap, worldPos.x);
        float ny = Mathf.InverseLerp(-halfMap, halfMap, worldPos.y);

        Vector2 size = mapRect.rect.size;
        dot.anchoredPosition = new Vector2(
            (nx - 0.5f) * size.x,
            (ny - 0.5f) * size.y);
    }
}
