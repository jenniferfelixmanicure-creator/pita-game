using UnityEngine;

/// <summary>Sistema de moedas e gemas — coleta, bônus e UI.</summary>
public class CurrencySystem : MonoBehaviour
{
    public static CurrencySystem Instance { get; private set; }

    private float coinBonusPercent = 0f;
    private float gemBonusPercent = 0f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddCoins(int amount)
    {
        int total = Mathf.RoundToInt(amount * (1f + coinBonusPercent / 100f));
        SaveSystem.Instance?.AddCoins(total);
        UIManager.Instance?.UpdateCurrencyUI(
            SaveSystem.Instance?.Data.coins ?? 0,
            SaveSystem.Instance?.Data.gems ?? 0);
    }

    public void AddGems(int amount)
    {
        int total = Mathf.RoundToInt(amount * (1f + gemBonusPercent / 100f));
        SaveSystem.Instance?.AddGems(total);
    }

    public void AddCoinBonusPercent(float bonus) => coinBonusPercent += bonus;
    public void AddGemBonusPercent(float bonus) => gemBonusPercent += bonus;

    public void RefreshUI()
    {
        UIManager.Instance?.UpdateCurrencyUI(
            SaveSystem.Instance?.Data.coins ?? 0,
            SaveSystem.Instance?.Data.gems ?? 0);
    }
}
