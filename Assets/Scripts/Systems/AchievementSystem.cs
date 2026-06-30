using UnityEngine;
using System.Collections.Generic;

/// <summary>Sistema de conquistas — 100 conquistas desbloqueáveis.</summary>
public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }

    [System.Serializable]
    public class Achievement
    {
        public int id;
        public string title;
        [TextArea] public string description;
        public Sprite icon;
        public int rewardCoins;
        public int rewardGems;
        public AchievementTrigger trigger;
        public int targetValue;
    }

    public enum AchievementTrigger
    {
        TotalKills, TotalRuns, BestScore, SurviveMinutes,
        ReachLevel, UnlockCharacters, OpenChests, SpendCoins,
        KillBoss, CompleteAllMissions, ReachSeasonLevel
    }

    [SerializeField] private List<Achievement> achievements;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnGameOver += CheckRunEndAchievements;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= CheckRunEndAchievements;
    }

    private void CheckRunEndAchievements()
    {
        var data = SaveSystem.Instance?.Data;
        if (data == null) return;

        data.TotalRuns++;
        data.TotalEnemiesKilled += GameManager.Instance?.EnemiesKilled ?? 0;

        CheckAll();
        SaveSystem.Instance.Save();
    }

    public void CheckAll()
    {
        foreach (var a in achievements)
            TryUnlock(a);
    }

    private void TryUnlock(Achievement a)
    {
        var data = SaveSystem.Instance?.Data;
        if (data == null || data.achievements[a.id]) return;

        bool unlocked = false;
        switch (a.trigger)
        {
            case AchievementTrigger.TotalKills:
                unlocked = data.TotalEnemiesKilled >= a.targetValue; break;
            case AchievementTrigger.TotalRuns:
                unlocked = data.TotalRuns >= a.targetValue; break;
            case AchievementTrigger.BestScore:
                unlocked = data.BestScore >= a.targetValue; break;
            case AchievementTrigger.SurviveMinutes:
                unlocked = data.BestSurvivalTime >= a.targetValue * 60f; break;
            case AchievementTrigger.KillBoss:
                unlocked = data.TotalEnemiesKilled >= a.targetValue; break;
        }

        if (unlocked) Unlock(a);
    }

    private void Unlock(Achievement a)
    {
        var data = SaveSystem.Instance.Data;
        data.achievements[a.id] = true;
        CurrencySystem.Instance?.AddCoins(a.rewardCoins);
        CurrencySystem.Instance?.AddGems(a.rewardGems);
        UIManager.Instance?.ShowAchievement(a.title, a.icon);
        AudioManager.Instance?.Play("achievement");
        SaveSystem.Instance.Save();
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        var data = SaveSystem.Instance?.Data;
        if (data == null) return 0;
        foreach (var b in data.achievements) if (b) count++;
        return count;
    }
}
