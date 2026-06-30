using UnityEngine;
using System.Collections;

/// <summary>
/// Item coletável no chão: XP, moedas, gemas, orbes de vida.
/// Flutua até o jogador quando dentro do raio de coleta.
/// </summary>
public class PickupItem : MonoBehaviour
{
    public enum PickupType { XP, Coin, Gem, HealthOrb, MagnetAll, Chest }

    [Header("Configurações")]
    [SerializeField] private PickupType type = PickupType.XP;
    [SerializeField] private float value = 10f;
    [SerializeField] private float attractSpeed = 8f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    private Transform target;
    private bool attracting;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
        StartCoroutine(Bob());
    }

    private void Update()
    {
        if (!attracting || target == null) return;
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, attractSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
            Collect();
    }

    public void Attract(Transform player)
    {
        if (attracting) return;
        attracting = true;
        target = player;
        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) Collect();
    }

    private void Collect()
    {
        switch (type)
        {
            case PickupType.XP:
                XPSystem.Instance?.AddXP(value);
                AudioManager.Instance?.Play("xp_collect");
                break;
            case PickupType.Coin:
                CurrencySystem.Instance?.AddCoins((int)value);
                AudioManager.Instance?.Play("coin_collect");
                break;
            case PickupType.Gem:
                CurrencySystem.Instance?.AddGems((int)value);
                AudioManager.Instance?.Play("gem_collect");
                break;
            case PickupType.HealthOrb:
                PlayerController.Instance?.Heal(value);
                AudioManager.Instance?.Play("heal_collect");
                break;
            case PickupType.MagnetAll:
                MagnetAll();
                break;
            case PickupType.Chest:
                ChestSystem.Instance?.OpenChest(ChestType.Silver);
                break;
        }
        Destroy(gameObject);
    }

    private void MagnetAll()
    {
        var all = FindObjectsOfType<PickupItem>();
        foreach (var p in all)
            if (p != this && PlayerController.Instance != null)
                p.Attract(PlayerController.Instance.transform);
    }

    private IEnumerator Bob()
    {
        float t = Random.Range(0f, Mathf.PI * 2f);
        while (true)
        {
            t += Time.deltaTime * bobSpeed;
            transform.position = startPos + Vector3.up * Mathf.Sin(t) * bobHeight;
            yield return null;
        }
    }
}
