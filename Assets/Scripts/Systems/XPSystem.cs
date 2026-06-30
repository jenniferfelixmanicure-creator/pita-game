using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Sistema de XP e level-up do jogador durante a partida.
/// Ao subir de nível, apresenta opções de habilidades para escolha.
/// </summary>
public class XPSystem : MonoBehaviour
{
    public static XPSystem Instance { get; private set; }

    // --- Configurações ---
    [Header("Configurações de XP")]
    [SerializeField] private AnimationCurve xpCurve; // XP necessário por nível
    [SerializeField] private int baseXPToLevelUp = 100;
    [SerializeField] private float xpCurveMultiplier = 1.3f;
    [SerializeField] private int maxLevel = 100;

    // --- Estado atual ---
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentXP { get; private set; } = 0f;
    public float XPToNextLevel { get; private set; }

    // --- Eventos ---
    public static event Action<int> OnLevelUp;
    public static event Action<float, float> OnXPChanged;

    // --- Cache de XP por nível ---
    private List<float> xpTable = new List<float>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        BuildXPTable();
    }

    private void Start()
    {
        XPToNextLevel = GetXPForLevel(CurrentLevel);
        OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
    }

    private void BuildXPTable()
    {
        xpTable.Clear();
        for (int i = 1; i <= maxLevel; i++)
        {
            float xp = baseXPToLevelUp * Mathf.Pow(xpCurveMultiplier, i - 1);
            xpTable.Add(xp);
        }
    }

    public void AddXP(float amount)
    {
        if (CurrentLevel >= maxLevel) return;

        // Bônus de XP de upgrades permanentes
        float xpBonus = 1f + (SaveSystem.Instance?.Data?.xpBonus ?? 0f);
        CurrentXP += amount * xpBonus;
        OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);

        while (CurrentXP >= XPToNextLevel && CurrentLevel < maxLevel)
        {
            CurrentXP -= XPToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        CurrentLevel++;
        XPToNextLevel = GetXPForLevel(CurrentLevel);

        OnLevelUp?.Invoke(CurrentLevel);
        AudioManager.Instance?.Play("level_up");
        GameManager.Instance?.Vibrate();

        // Apresentar escolha de habilidades
        AbilitySelectionUI.Instance?.ShowAbilityChoices(3);

        // Pausar o tempo durante a seleção
        Time.timeScale = 0f;

        UIManager.Instance?.ShowLevelUpEffect(CurrentLevel);
    }

    private float GetXPForLevel(int level)
    {
        int idx = Mathf.Clamp(level - 1, 0, xpTable.Count - 1);
        return xpTable.Count > 0 ? xpTable[idx] : baseXPToLevelUp;
    }

    public float GetXPPercent()
    {
        if (XPToNextLevel <= 0) return 1f;
        return Mathf.Clamp01(CurrentXP / XPToNextLevel);
    }

    public void OnAbilitySelected()
    {
        // Retomar o jogo após a seleção
        Time.timeScale = 1f;
    }
}
