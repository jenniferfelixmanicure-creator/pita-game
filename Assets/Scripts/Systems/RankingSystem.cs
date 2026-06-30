using UnityEngine;
using System.Collections.Generic;

/// <summary>Ranking local + preparado para Firebase Leaderboard online.</summary>
public class RankingSystem : MonoBehaviour
{
    public static RankingSystem Instance { get; private set; }

    [System.Serializable]
    public class RankEntry
    {
        public string playerName;
        public int score;
        public int wave;
        public int level;
        public float survivalTime;
        public string characterId;
        public string dateStr;
    }

    private List<RankEntry> localBoard = new List<RankEntry>();
    private const int MAX_ENTRIES = 100;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    public void SubmitScore(int score, int wave, float time, string characterId)
    {
        var entry = new RankEntry
        {
            playerName  = SaveSystem.Instance?.Data?.playerName ?? "Player",
            score       = score,
            wave        = wave,
            level       = XPSystem.Instance?.CurrentLevel ?? 1,
            survivalTime = time,
            characterId = characterId,
            dateStr     = System.DateTime.Now.ToString("dd/MM/yy")
        };

        localBoard.Add(entry);
        localBoard.Sort((a, b) => b.score.CompareTo(a.score));
        if (localBoard.Count > MAX_ENTRIES) localBoard.RemoveAt(localBoard.Count - 1);

        Save();
        FirebaseManager.Instance?.SubmitScore(entry.playerName, score);
    }

    public List<RankEntry> GetTopEntries(int count = 10) =>
        localBoard.GetRange(0, Mathf.Min(count, localBoard.Count));

    public int GetLocalRank(int score)
    {
        for (int i = 0; i < localBoard.Count; i++)
            if (localBoard[i].score <= score) return i + 1;
        return localBoard.Count + 1;
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(new Wrapper { entries = localBoard });
        PlayerPrefs.SetString("LocalRanking", json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        string json = PlayerPrefs.GetString("LocalRanking", "");
        if (!string.IsNullOrEmpty(json))
        {
            var w = JsonUtility.FromJson<Wrapper>(json);
            if (w?.entries != null) localBoard = w.entries;
        }
    }

    [System.Serializable] private class Wrapper { public List<RankEntry> entries; }
}
