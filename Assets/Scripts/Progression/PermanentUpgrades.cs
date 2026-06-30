using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de upgrades permanentes (persistem entre partidas).
/// O jogador compra com moedas/gemas na tela de progressão.
/// </summary>
public class PermanentUpgrades : MonoBehaviour
{
    public static PermanentUpgrades Instance { get; private set; }

    [System.Serializable]
    public class UpgradeDefinition
    {
        public int id;
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;
        public int maxLevel = 10;
        public int[] coinCostPerLevel;     // Custo em moedas por nível
        public int[] gemCostPerLevel;      // Custo em gemas por nível
        public UpgradeCategory category;
        public UpgradeEffect effect;
        public float valuePerLevel = 1f;   // Quanto muda por nível
    }

    public enum UpgradeCategory
    {
        Combat, Defense, Utility, Economy, Special
    }

    public enum UpgradeEffect
    {
        MaxHP, MoveSpeed, Damage, Defense, AttackSpeed,
        XPGain, CoinGain, StartCoins, PickupRadius,
        CritChance, CritDamage, HealOnKill, ShieldRegen,
        ReduceEnemyHP, IncreaseAbilitySlots, ReviveChance,
        GemGain, BonusLives, AbilityStartLevel, MagnetPower
    }

    [Header("Upgrades Disponíveis")]
    [SerializeField] private List<UpgradeDefinition> upgrades;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // --- Comprar upgrade ---

    public bool PurchaseUpgrade(int upgradeId)
    {
        var def = GetUpgrade(upgradeId);
        if (def == null) return false;

        int currentLevel = SaveSystem.Instance.GetUpgradeLevel(upgradeId);
        if (currentLevel >= def.maxLevel) return false;

        int coinCost = GetCoinCost(def, currentLevel);
        int gemCost = GetGemCost(def, currentLevel);

        if (!SaveSystem.Instance.SpendCoins(coinCost)) return false;
        if (gemCost > 0 && !SaveSystem.Instance.SpendGems(gemCost))
        {
            SaveSystem.Instance.AddCoins(coinCost); // Devolver
            return false;
        }

        SaveSystem.Instance.SetUpgradeLevel(upgradeId, currentLevel + 1);
        AudioManager.Instance?.Play("upgrade_purchased");
        return true;
    }

    // --- Aplicar upgrades ao iniciar partida ---

    public void ApplyAllUpgrades(PlayerController player)
    {
        foreach (var def in upgrades)
        {
            int level = SaveSystem.Instance.GetUpgradeLevel(def.id);
            if (level <= 0) continue;

            float totalValue = def.valuePerLevel * level;
            ApplyEffect(player, def.effect, totalValue);
        }
    }

    private void ApplyEffect(PlayerController player, UpgradeEffect effect, float value)
    {
        switch (effect)
        {
            case UpgradeEffect.MaxHP:          player.bonusMaxHealth += value; break;
            case UpgradeEffect.MoveSpeed:      player.bonusMoveSpeed += value; break;
            case UpgradeEffect.Damage:         player.bonusDamage += value / 100f; break;
            case UpgradeEffect.Defense:        player.bonusDefense += value; break;
            case UpgradeEffect.AttackSpeed:    player.bonusAttackSpeed += value / 100f; break;
            case UpgradeEffect.XPGain:         SaveSystem.Instance.Data.xpBonus += value / 100f; break;
            case UpgradeEffect.PickupRadius:   player.bonusPickupRadius += value; break;
            case UpgradeEffect.CritChance:     CombatStats.Instance?.AddCritChance(value); break;
            case UpgradeEffect.CritDamage:     CombatStats.Instance?.AddCritMultiplier(value / 100f); break;
            case UpgradeEffect.CoinGain:       CurrencySystem.Instance?.AddCoinBonusPercent(value); break;
            case UpgradeEffect.StartCoins:     CurrencySystem.Instance?.AddCoins((int)value); break;
        }
    }

    // --- Utilitários ---

    public UpgradeDefinition GetUpgrade(int id) =>
        upgrades?.Find(u => u.id == id);

    public List<UpgradeDefinition> GetByCategory(UpgradeCategory cat) =>
        upgrades?.FindAll(u => u.category == cat);

    private int GetCoinCost(UpgradeDefinition def, int currentLevel)
    {
        if (def.coinCostPerLevel != null && currentLevel < def.coinCostPerLevel.Length)
            return def.coinCostPerLevel[currentLevel];
        return 100 * (currentLevel + 1);
    }

    private int GetGemCost(UpgradeDefinition def, int currentLevel)
    {
        if (def.gemCostPerLevel != null && currentLevel < def.gemCostPerLevel.Length)
            return def.gemCostPerLevel[currentLevel];
        return 0;
    }

    public bool CanAfford(int upgradeId)
    {
        var def = GetUpgrade(upgradeId);
        if (def == null) return false;
        int level = SaveSystem.Instance.GetUpgradeLevel(upgradeId);
        return SaveSystem.Instance.Data.coins >= GetCoinCost(def, level);
    }
}
