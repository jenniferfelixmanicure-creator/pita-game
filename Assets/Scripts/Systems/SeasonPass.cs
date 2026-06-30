using UnityEngine;
using System.Collections.Generic;

/// <summary>Passe de temporada com 50 níveis e recompensas gratuitas + premium.</summary>
public class SeasonPass : MonoBehaviour
{
    public static SeasonPass Instance { get; private set; }

    [System.Serializable]
    public class SeasonReward
    {
        public int level;
        public int freeCoins;
        public int freeGems;
        public int premiumCoins;
        public int premiumGems;
        public string premiumSkinId;
        public bool isMilestone;
    }

    [Header("Passe de Temporada")]
    [SerializeField] private List<SeasonReward> rewards;
    [SerializeField] private int xpPerKill = 2;
    [SerializeField] private int xpPerBoss = 50;
    [SerializeField] private int maxLevel = 50;
    [SerializeField] private int passGemCost = 400;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() => GameManager.OnGameOver += OnRunEnded;
    private void OnDisable() => GameManager.OnGameOver -= OnRunEnded;

    private void OnRunEnded()
    {
        var data = SaveSystem.Instance?.Data;
        if (data == null) return;

        int xpGained = (GameManager.Instance?.EnemiesKilled ?? 0) * xpPerKill;
        data.seasonXP += xpGained;

        int newLevel = CalculateLevel(data.seasonXP);
        if (newLevel > data.seasonPassLevel)
        {
            for (int l = data.seasonPassLevel + 1; l <= newLevel; l++)
                GrantLevelReward(l, data.hasSeasonPass);
            data.seasonPassLevel = newLevel;
            SaveSystem.Instance.Save();
        }
    }

    private int CalculateLevel(int xp)
    {
        int level = 0;
        int required = 100;
        int remaining = xp;
        while (remaining >= required && level < maxLevel)
        {
            remaining -= required;
            level++;
            required = Mathf.RoundToInt(required * 1.1f);
        }
        return level;
    }

    private void GrantLevelReward(int level, bool hasPremium)
    {
        var r = rewards?.Find(x => x.level == level);
        if (r == null) return;

        CurrencySystem.Instance?.AddCoins(r.freeCoins);
        CurrencySystem.Instance?.AddGems(r.freeGems);

        if (hasPremium)
        {
            CurrencySystem.Instance?.AddCoins(r.premiumCoins);
            CurrencySystem.Instance?.AddGems(r.premiumGems);
        }

        UIManager.Instance?.ShowSeasonLevelUp(level, r.isMilestone);
        AudioManager.Instance?.Play(r.isMilestone ? "achievement" : "coin_collect");
    }

    public bool PurchasePass()
    {
        var data = SaveSystem.Instance?.Data;
        if (data == null || data.hasSeasonPass) return false;
        if (!SaveSystem.Instance.SpendGems(passGemCost)) return false;
        data.hasSeasonPass = true;
        SaveSystem.Instance.Save();
        return true;
    }

    public float GetProgressPercent()
    {
        var data = SaveSystem.Instance?.Data;
        if (data == null) return 0f;
        return Mathf.Clamp01((float)data.seasonPassLevel / maxLevel);
    }
}
