using UnityEngine;
using System.Collections.Generic;

/// <summary>Sistema de missões diárias, semanais e permanentes.</summary>
public class MissionSystem : MonoBehaviour
{
    public static MissionSystem Instance { get; private set; }

    [System.Serializable]
    public class Mission
    {
        public int id;
        public string title;
        public string description;
        public MissionType type;
        public string trackingKey; // ex: "MeleeEnemy", "run", "coins"
        public int target;
        public int rewardCoins;
        public int rewardGems;
        public MissionFrequency frequency;
    }

    public enum MissionType { Kill, Survive, Collect, LevelUp, Win }
    public enum MissionFrequency { Daily, Weekly, Permanent }

    [SerializeField] private List<Mission> missions;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterKill(string enemyType)
    {
        foreach (var m in missions)
        {
            if (m.type == MissionType.Kill && m.trackingKey == enemyType)
                IncrementProgress(m.id);
        }
        // Missão genérica "matar X inimigos"
        foreach (var m in missions)
        {
            if (m.type == MissionType.Kill && m.trackingKey == "Any")
                IncrementProgress(m.id);
        }
    }

    private void IncrementProgress(int missionId)
    {
        var save = SaveSystem.Instance?.Data;
        if (save == null || missionId >= save.missionProgress.Length) return;
        if (save.completedMissions[missionId]) return;

        save.missionProgress[missionId]++;

        var m = missions.Find(x => x.id == missionId);
        if (m != null && save.missionProgress[missionId] >= m.target)
            CompleteMission(m);
    }

    private void CompleteMission(Mission m)
    {
        var save = SaveSystem.Instance?.Data;
        if (save == null) return;
        save.completedMissions[m.id] = true;
        CurrencySystem.Instance?.AddCoins(m.rewardCoins);
        CurrencySystem.Instance?.AddGems(m.rewardGems);
        UIManager.Instance?.ShowMissionComplete(m.title);
        AudioManager.Instance?.Play("mission_complete");
        SaveSystem.Instance?.Save();
    }

    public int GetProgress(int missionId) =>
        SaveSystem.Instance?.Data?.missionProgress[missionId] ?? 0;

    public bool IsCompleted(int missionId) =>
        SaveSystem.Instance?.Data?.completedMissions[missionId] ?? false;
}
