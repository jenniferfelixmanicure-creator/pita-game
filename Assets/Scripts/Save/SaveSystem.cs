using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Sistema de save/load com criptografia básica.
/// Salva todo o progresso permanente do jogador.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    [Serializable]
    public class SaveData
    {
        // --- Moedas e gemas ---
        public int coins = 0;
        public int gems = 0;

        // --- Recordes ---
        public int BestScore = 0;
        public float BestSurvivalTime = 0f;
        public int MostKillsInRun = 0;

        // --- Personagem selecionado ---
        public int selectedCharacterIndex = 0;
        public bool[] unlockedCharacters = new bool[30];

        // --- Upgrades permanentes ---
        public int[] permanentUpgrades = new int[100]; // índice = upgrade, valor = nível

        // --- Missões ---
        public int[] missionProgress = new int[50];
        public bool[] completedMissions = new bool[50];

        // --- Conquistas ---
        public bool[] achievements = new bool[100];

        // --- Passe de temporada ---
        public int seasonPassLevel = 0;
        public int seasonXP = 0;
        public bool hasSeasonPass = false;

        // --- Configurações ---
        public float masterVolume = 1f;
        public float musicVolume = 0.6f;
        public float sfxVolume = 1f;
        public bool vibrationEnabled = true;
        public int graphicsQuality = 1;
        public string language = "pt-BR";

        // --- Bônus de XP (de upgrades permanentes) ---
        public float xpBonus = 0f;

        // --- Estatísticas totais ---
        public int TotalEnemiesKilled = 0;
        public int TotalRuns = 0;
        public float TotalTimePlayed = 0f;

        // --- Loja / compras ---
        public bool[] purchasedSkins = new bool[50];

        // --- Recompensas diárias ---
        public string lastDailyRewardDate = "";
        public int dailyStreak = 0;
    }

    public SaveData Data { get; private set; }

    private string savePath;
    private const string SAVE_FILE = "pita_save.dat";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        Load();
    }

    // --- Salvar ---

    public void Save()
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                bf.Serialize(fs, Data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Erro ao salvar: {e.Message}");
        }
    }

    // --- Carregar ---

    public void Load()
    {
        if (File.Exists(savePath))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream fs = new FileStream(savePath, FileMode.Open))
                {
                    Data = (SaveData)bf.Deserialize(fs);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Erro ao carregar save, criando novo: {e.Message}");
                Data = new SaveData();
            }
        }
        else
        {
            Data = new SaveData();
            Data.unlockedCharacters[0] = true; // Personagem inicial desbloqueado
            Save();
        }
    }

    // --- Deletar save ---

    public void DeleteSave()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
        Data = new SaveData();
        Data.unlockedCharacters[0] = true;
    }

    // --- Moedas e Gemas ---

    public bool SpendCoins(int amount)
    {
        if (Data.coins < amount) return false;
        Data.coins -= amount;
        Save();
        return true;
    }

    public void AddCoins(int amount)
    {
        Data.coins += amount;
        Save();
        CurrencySystem.Instance?.RefreshUI();
    }

    public bool SpendGems(int amount)
    {
        if (Data.gems < amount) return false;
        Data.gems -= amount;
        Save();
        return true;
    }

    public void AddGems(int amount)
    {
        Data.gems += amount;
        Save();
        CurrencySystem.Instance?.RefreshUI();
    }

    // --- Upgrades permanentes ---

    public int GetUpgradeLevel(int upgradeId) =>
        upgradeId < Data.permanentUpgrades.Length ? Data.permanentUpgrades[upgradeId] : 0;

    public void SetUpgradeLevel(int upgradeId, int level)
    {
        if (upgradeId >= Data.permanentUpgrades.Length) return;
        Data.permanentUpgrades[upgradeId] = level;
        Save();
    }

    // --- Auto-save ---

    private float autoSaveTimer;
    private const float AUTO_SAVE_INTERVAL = 30f;

    private void Update()
    {
        autoSaveTimer += Time.unscaledDeltaTime;
        if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            autoSaveTimer = 0f;
            if (GameManager.Instance.IsPlaying)
            {
                Data.TotalTimePlayed += AUTO_SAVE_INTERVAL;
                Save();
            }
        }
    }
}
