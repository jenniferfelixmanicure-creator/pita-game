using UnityEngine;

public enum ChestType { Bronze, Silver, Gold, Boss, Season }

/// <summary>Sistema de baús com recompensas por tipo e raridade.</summary>
public class ChestSystem : MonoBehaviour
{
    public static ChestSystem Instance { get; private set; }

    [System.Serializable]
    public class ChestReward
    {
        public ChestType type;
        public int minCoins, maxCoins;
        public int minGems, maxGems;
        public float abilityUpgradeChance;
    }

    [SerializeField] private ChestReward[] rewards;
    [SerializeField] private GameObject chestDropPrefab;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void DropChest(Vector3 pos, ChestType type)
    {
        if (chestDropPrefab)
        {
            var obj = Instantiate(chestDropPrefab, pos, Quaternion.identity);
            obj.GetComponent<DroppedChest>()?.SetType(type);
        }
    }

    public void OpenChest(ChestType type)
    {
        var r = System.Array.Find(rewards, x => x.type == type);
        if (r == null) return;

        int coins = Random.Range(r.minCoins, r.maxCoins + 1);
        int gems  = Random.Range(r.minGems,  r.maxGems  + 1);

        CurrencySystem.Instance?.AddCoins(coins);
        CurrencySystem.Instance?.AddGems(gems);
        AudioManager.Instance?.Play("chest_open");

        if (Random.value < r.abilityUpgradeChance && GameManager.Instance.IsPlaying)
            AbilitySelectionUI.Instance?.ShowAbilityChoices(1);

        UIManager.Instance?.ShowChestReward(coins, gems);
    }
}

/// <summary>Baú largado no chão — o jogador coleta e abre.</summary>
public class DroppedChest : MonoBehaviour
{
    private ChestType chestType;

    public void SetType(ChestType t) => chestType = t;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        ChestSystem.Instance?.OpenChest(chestType);
        Destroy(gameObject);
    }
}
