using UnityEngine;
using System;

/// <summary>Sistema de recompensas diárias com streak progressivo.</summary>
public class DailyRewardSystem : MonoBehaviour
{
    public static DailyRewardSystem Instance { get; private set; }

    [System.Serializable]
    public class DailyReward
    {
        public int day;
        public int coins;
        public int gems;
        public bool isSpecial;
    }

    [SerializeField] private DailyReward[] rewards; // 7 dias de ciclo

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool CanClaimToday()
    {
        var data = SaveSystem.Instance?.Data;
        if (data == null) return false;
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        return data.lastDailyRewardDate != today;
    }

    public void ClaimReward()
    {
        if (!CanClaimToday()) return;

        var data = SaveSystem.Instance.Data;
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        // Streak: continua só se ontem coletou
        if (data.lastDailyRewardDate == yesterday)
            data.dailyStreak++;
        else if (data.lastDailyRewardDate != today)
            data.dailyStreak = 1;

        data.lastDailyRewardDate = today;

        int idx = (data.dailyStreak - 1) % rewards.Length;
        var reward = rewards[idx];

        CurrencySystem.Instance?.AddCoins(reward.coins);
        CurrencySystem.Instance?.AddGems(reward.gems);

        UIManager.Instance?.ShowDailyReward(reward.day, reward.coins, reward.gems, reward.isSpecial);
        AudioManager.Instance?.Play(reward.isSpecial ? "gem_collect" : "coin_collect");

        SaveSystem.Instance.Save();
    }

    public int GetCurrentStreak() => SaveSystem.Instance?.Data?.dailyStreak ?? 0;
    public int GetNextRewardDay() => (GetCurrentStreak() % (rewards?.Length ?? 7)) + 1;
}
