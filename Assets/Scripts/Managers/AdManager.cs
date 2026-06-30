using UnityEngine;
using System;

/// <summary>
/// Gerenciador de anúncios — integra com Google AdMob (via Unity Ads SDK).
/// Em produção, substituir os métodos stub pelos da SDK real.
/// </summary>
public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("Ad Unit IDs (AdMob)")]
    [SerializeField] private string interstitialId  = "ca-app-pub-XXXXXX/XXXXXX";
    [SerializeField] private string rewardedId      = "ca-app-pub-XXXXXX/XXXXXX";
    [SerializeField] private string bannerBottomId  = "ca-app-pub-XXXXXX/XXXXXX";

    [Header("Frequência")]
    [SerializeField] private int runsPerInterstitial = 3;   // Mostra inter a cada N runs
    [SerializeField] private bool showBannerOnMenu = true;

    private int runsThisSession = 0;
    private bool isAdFree => SaveSystem.Instance?.Data?.hasAdFree ?? false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAds();
    }

    private void InitializeAds()
    {
        if (isAdFree) return;
        // TODO: MobileAds.Initialize(initStatus => { });
        Debug.Log("[AdManager] AdMob inicializado (stub).");
    }

    public void ShowBanner()
    {
        if (isAdFree) return;
        // TODO: bannerView?.Show();
        Debug.Log("[AdManager] Banner exibido.");
    }

    public void HideBanner()
    {
        // TODO: bannerView?.Hide();
    }

    public void ShowInterstitial(Action onClosed = null)
    {
        if (isAdFree) { onClosed?.Invoke(); return; }
        runsThisSession++;
        if (runsThisSession % runsPerInterstitial != 0) { onClosed?.Invoke(); return; }

        // TODO: interstitial?.Show();
        Debug.Log("[AdManager] Intersticial exibido (stub).");
        onClosed?.Invoke();
    }

    public void ShowRewarded(Action onSuccess, Action onFailed = null)
    {
        if (isAdFree) { onSuccess?.Invoke(); return; }

        // TODO: rewardedAd?.Show(reward => onSuccess?.Invoke());
        // Stub: simula sempre sucesso em desenvolvimento
        Debug.Log("[AdManager] Rewarded exibido (stub) — recompensa concedida.");
        onSuccess?.Invoke();
    }

    public void ShowRewardedForCoins(int amount)
    {
        ShowRewarded(() =>
        {
            CurrencySystem.Instance?.AddCoins(amount);
            UIManager.Instance?.ShowMessage($"+{amount} Moedas!");
            AudioManager.Instance?.Play("coin_collect");
        });
    }

    public void ShowRewardedForRevive()
    {
        ShowRewarded(() =>
        {
            PlayerController.Instance?.Revive();
            UIManager.Instance?.HideRevivePanel();
            AudioManager.Instance?.Play("heal_collect");
        });
    }

    public void ShowRewardedForChest()
    {
        ShowRewarded(() => ChestSystem.Instance?.OpenChest(ChestType.Gold));
    }

    public void PurchaseAdFree()
    {
        // TODO: integrar com Unity IAP
        if (SaveSystem.Instance?.Data == null) return;
        SaveSystem.Instance.Data.hasAdFree = true;
        SaveSystem.Instance.Save();
        HideBanner();
        UIManager.Instance?.ShowMessage("Jogo sem anúncios ativado! Obrigado!");
    }
}
