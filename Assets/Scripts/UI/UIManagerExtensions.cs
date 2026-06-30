using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Extensão do UIManager — métodos de notificações, toasts e popups.
/// Todos os métodos são chamados pelos sistemas de jogo (ChestSystem, AchievementSystem, etc.)
/// </summary>
public partial class UIManager : MonoBehaviour
{
    [Header("Notificações e Popups")]
    [SerializeField] private GameObject toastPrefab;
    [SerializeField] private Transform toastContainer;

    [SerializeField] private GameObject achievementPopup;
    [SerializeField] private Image achievementIcon;
    [SerializeField] private TextMeshProUGUI achievementTitle;

    [SerializeField] private GameObject chestRewardPopup;
    [SerializeField] private TextMeshProUGUI chestCoinsText;
    [SerializeField] private TextMeshProUGUI chestGemsText;

    [SerializeField] private GameObject dailyRewardPopup;
    [SerializeField] private TextMeshProUGUI dailyDayText;
    [SerializeField] private TextMeshProUGUI dailyCoinsText;
    [SerializeField] private TextMeshProUGUI dailyGemsText;
    [SerializeField] private GameObject dailySpecialBadge;
    [SerializeField] private Button claimDailyButton;

    [SerializeField] private GameObject missionCompletePopup;
    [SerializeField] private TextMeshProUGUI missionNameText;
    [SerializeField] private TextMeshProUGUI missionRewardText;

    [SerializeField] private GameObject seasonLevelPopup;
    [SerializeField] private TextMeshProUGUI seasonLevelText;

    [SerializeField] private GameObject confirmDialog;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [SerializeField] private GameObject revivePanel;
    [SerializeField] private TextMeshProUGUI reviveCountdownText;

    // ── Toast / Mensagem rápida ─────────────────────────────────────────────────

    public void ShowMessage(string msg)
    {
        if (toastPrefab && toastContainer)
        {
            var toast = Instantiate(toastPrefab, toastContainer);
            var tmp = toast.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp) tmp.text = msg;
            StartCoroutine(DestroyAfter(toast, 2.5f));
        }
        else
        {
            Debug.Log($"[UI] {msg}");
        }
    }

    private IEnumerator DestroyAfter(GameObject obj, float t)
    {
        yield return new WaitForSeconds(t);
        if (obj) Destroy(obj);
    }

    // ── Conquista desbloqueada ─────────────────────────────────────────────────

    public void ShowAchievement(string title, Sprite icon)
    {
        if (achievementPopup)
        {
            achievementPopup.SetActive(true);
            if (achievementTitle) achievementTitle.text = title;
            if (achievementIcon && icon) achievementIcon.sprite = icon;
            StartCoroutine(HideAfter(achievementPopup, 3.5f));
        }
    }

    // ── Baú aberto ─────────────────────────────────────────────────────────────

    public void ShowChestReward(int coins, int gems)
    {
        if (chestRewardPopup)
        {
            chestRewardPopup.SetActive(true);
            if (chestCoinsText) chestCoinsText.text = $"+{coins}";
            if (chestGemsText) chestGemsText.text = $"+{gems}";
            StartCoroutine(HideAfter(chestRewardPopup, 3f));
        }
    }

    // ── Recompensa diária ──────────────────────────────────────────────────────

    public void ShowDailyReward(int day, int coins, int gems, bool special)
    {
        if (dailyRewardPopup)
        {
            dailyRewardPopup.SetActive(true);
            if (dailyDayText) dailyDayText.text = $"Dia {day}";
            if (dailyCoinsText) dailyCoinsText.text = $"+{coins}";
            if (dailyGemsText) dailyGemsText.text = $"+{gems}";
            if (dailySpecialBadge) dailySpecialBadge.SetActive(special);
            claimDailyButton?.onClick.RemoveAllListeners();
            claimDailyButton?.onClick.AddListener(() =>
            {
                DailyRewardSystem.Instance?.ClaimReward();
                dailyRewardPopup.SetActive(false);
            });
        }
    }

    // ── Missão concluída ───────────────────────────────────────────────────────

    public void ShowMissionComplete(string missionName, int rewardCoins)
    {
        if (missionCompletePopup)
        {
            missionCompletePopup.SetActive(true);
            if (missionNameText) missionNameText.text = missionName;
            if (missionRewardText) missionRewardText.text = $"+{rewardCoins} Moedas";
            StartCoroutine(HideAfter(missionCompletePopup, 3f));
        }
    }

    // ── Season pass nível ──────────────────────────────────────────────────────

    public void ShowSeasonLevelUp(int level, bool milestone)
    {
        if (seasonLevelPopup)
        {
            seasonLevelPopup.SetActive(true);
            if (seasonLevelText) seasonLevelText.text = $"Passe Nível {level}{(milestone ? " ★" : "")}";
            StartCoroutine(HideAfter(seasonLevelPopup, 3f));
        }
    }

    // ── Dialog de confirmação ──────────────────────────────────────────────────

    public void ShowConfirmDialog(string msg, Action onConfirm)
    {
        if (!confirmDialog) { onConfirm?.Invoke(); return; }
        confirmDialog.SetActive(true);
        if (confirmText) confirmText.text = msg;

        confirmYesButton?.onClick.RemoveAllListeners();
        confirmYesButton?.onClick.AddListener(() =>
        {
            confirmDialog.SetActive(false);
            onConfirm?.Invoke();
        });
        confirmNoButton?.onClick.RemoveAllListeners();
        confirmNoButton?.onClick.AddListener(() => confirmDialog.SetActive(false));
    }

    // ── Revive ─────────────────────────────────────────────────────────────────

    public void ShowRevivePanel(float countdownSecs)
    {
        if (revivePanel) revivePanel.SetActive(true);
        StartCoroutine(ReviveCountdown(countdownSecs));
    }

    public void HideRevivePanel()
    {
        if (revivePanel) revivePanel.SetActive(false);
    }

    private IEnumerator ReviveCountdown(float duration)
    {
        float t = duration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            if (reviveCountdownText) reviveCountdownText.text = Mathf.CeilToInt(t).ToString();
            yield return null;
        }
        HideRevivePanel();
        GameManager.Instance?.EndGame();
    }

    // ── Shield ─────────────────────────────────────────────────────────────────

    public void ShowShieldIndicator(bool active)
    {
        // Implementado na parte principal do UIManager — este método garante compatibilidade
    }

    private IEnumerator HideAfter(GameObject obj, float t)
    {
        yield return new WaitForSeconds(t);
        if (obj) obj.SetActive(false);
    }
}
