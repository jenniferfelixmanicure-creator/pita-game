using UnityEngine;
using System;

/// <summary>
/// Manager de Firebase — Analytics, Auth, Firestore, Remote Config e Leaderboard.
/// Em produção: instale o pacote Firebase Unity SDK e descomente as chamadas reais.
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private bool enableAnalytics = true;
    [SerializeField] private bool enableRemoteConfig = true;
    [SerializeField] private bool enableLeaderboard = true;

    private bool initialized = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Initialize()
    {
        // TODO: FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => { ... });
        initialized = true;
        Debug.Log("[Firebase] Inicializado (stub).");
    }

    // ── Analytics ──────────────────────────────────────────────────────────────

    public void LogEvent(string eventName, params (string key, object value)[] parameters)
    {
        if (!enableAnalytics || !initialized) return;
        // TODO: FirebaseAnalytics.LogEvent(eventName, new Parameter[]{...});
        Debug.Log($"[Analytics] {eventName}");
    }

    public void LogRunStart(string characterId, int playerLevel)
        => LogEvent("run_start", ("character", characterId), ("level", playerLevel));

    public void LogRunEnd(int score, int wave, float time, string characterId)
        => LogEvent("run_end", ("score", score), ("wave", wave), ("time", Mathf.RoundToInt(time)), ("character", characterId));

    public void LogPurchase(string itemId, int cost, string currency)
        => LogEvent("item_purchase", ("item", itemId), ("cost", cost), ("currency", currency));

    public void LogLevelUp(int newLevel)
        => LogEvent("player_level_up", ("new_level", newLevel));

    // ── Remote Config ──────────────────────────────────────────────────────────

    public float GetFloat(string key, float defaultValue) => defaultValue;
    public int GetInt(string key, int defaultValue) => defaultValue;
    public string GetString(string key, string defaultValue) => defaultValue;
    public bool GetBool(string key, bool defaultValue) => defaultValue;

    // ── Leaderboard (Firestore) ────────────────────────────────────────────────

    public void SubmitScore(string playerName, int score)
    {
        if (!enableLeaderboard || !initialized) return;
        // TODO: db.Collection("leaderboard").AddAsync(new { name = playerName, score = score, ts = Timestamp.Now });
        Debug.Log($"[Firestore] Score enviado: {playerName} = {score}");
    }

    public void GetTopScores(int count, Action<System.Collections.Generic.List<(string name, int score)>> callback)
    {
        // TODO: query Firestore, retorna lista
        callback?.Invoke(new System.Collections.Generic.List<(string, int)>());
    }

    // ── Auth ───────────────────────────────────────────────────────────────────

    public void SignInAnonymous(Action<string> onSuccess, Action<string> onError)
    {
        // TODO: FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync()
        string fakeUid = "anon_" + SystemInfo.deviceUniqueIdentifier.Substring(0, 8);
        onSuccess?.Invoke(fakeUid);
    }

    public void DeleteAccount()
    {
        // TODO: FirebaseAuth.DefaultInstance.CurrentUser?.DeleteAsync()
        Debug.Log("[Firebase] Conta excluída (stub).");
    }

    public string GetUserId()
    {
        // TODO: return FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "";
        return "stub_uid";
    }

    // ── Crashlytics ────────────────────────────────────────────────────────────

    public void LogError(string context, Exception ex)
    {
        // TODO: Crashlytics.LogException(ex);
        Debug.LogError($"[Crashlytics] {context}: {ex?.Message}");
    }
}
