using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Gerencia toda a interface do jogador durante a partida e nos menus.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // --- HUD da Partida ---
    [Header("HUD")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider xpBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private RawImage minimap;

    // --- Painéis ---
    [Header("Painéis")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private GameObject bossWarningPanel;
    [SerializeField] private GameObject shieldIcon;
    [SerializeField] private GameObject abilitySelectionPanel;

    // --- Game Over ---
    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI goScoreText;
    [SerializeField] private TextMeshProUGUI goTimeText;
    [SerializeField] private TextMeshProUGUI goKillsText;
    [SerializeField] private TextMeshProUGUI goBestScoreText;

    // --- Level Up ---
    [Header("Level Up")]
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private Animator levelUpAnimator;

    // --- Boss Warning ---
    [SerializeField] private Animator bossWarningAnimator;

    // --- Boss HP Bar ---
    [Header("Boss")]
    [SerializeField] private GameObject bossHPContainer;
    [SerializeField] private Slider bossHPBar;
    [SerializeField] private TextMeshProUGUI bossNameText;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        XPSystem.OnXPChanged += UpdateXPBar;
        XPSystem.OnLevelUp += OnLevelUp;
        GameManager.OnGameOver += ShowGameOver;
        GameManager.OnGamePaused += ShowPause;
        GameManager.OnGameResumed += HidePause;
    }

    private void OnDisable()
    {
        XPSystem.OnXPChanged -= UpdateXPBar;
        XPSystem.OnLevelUp -= OnLevelUp;
        GameManager.OnGameOver -= ShowGameOver;
        GameManager.OnGamePaused -= ShowPause;
        GameManager.OnGameResumed -= HidePause;
    }

    private void Update()
    {
        UpdateTimer();
    }

    // --- HUD ---

    public void UpdateHealth(float current, float max)
    {
        if (healthBar) healthBar.value = current / max;
    }

    private void UpdateXPBar(float current, float max)
    {
        if (xpBar) xpBar.value = max > 0 ? current / max : 0f;
    }

    private void UpdateTimer()
    {
        if (timerText == null) return;
        float t = WaveManager.Instance?.GetGameTime() ?? 0f;
        int min = (int)(t / 60f);
        int sec = (int)(t % 60f);
        timerText.text = $"{min:00}:{sec:00}";
    }

    public void UpdateKillCount(int kills)
    {
        if (killCountText) killCountText.text = kills.ToString("N0");
    }

    public void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = score.ToString("N0");
    }

    public void UpdateCurrencyUI(int coins, int gems)
    {
        if (coinsText) coinsText.text = coins.ToString("N0");
        if (gemsText) gemsText.text = gems.ToString("N0");
    }

    private void OnLevelUp(int level)
    {
        if (levelText) levelText.text = $"Nv. {level}";
        ShowLevelUpEffect(level);
    }

    public void ShowLevelUpEffect(int level)
    {
        if (levelUpPanel) levelUpPanel.SetActive(true);
        if (levelUpText) levelUpText.text = $"NÍVEL {level}!";
        levelUpAnimator?.SetTrigger("LevelUp");
        StartCoroutine(HideAfter(levelUpPanel, 2f));
    }

    // --- Pausa ---

    private void ShowPause() => SetPanel(pausePanel, true);
    private void HidePause() => SetPanel(pausePanel, false);

    public void OnPauseButtonClicked()
    {
        if (GameManager.Instance.IsPlaying) GameManager.Instance.PauseGame();
        else if (GameManager.Instance.IsPaused) GameManager.Instance.ResumeGame();
    }

    // --- Game Over ---

    private void ShowGameOver()
    {
        SetPanel(gameOverPanel, true);
        SetPanel(hudPanel, false);

        if (goScoreText) goScoreText.text = $"Pontuação: {GameManager.Instance.CurrentScore:N0}";
        float t = WaveManager.Instance?.GetGameTime() ?? 0f;
        if (goTimeText) goTimeText.text = $"Tempo: {(int)(t / 60f):00}:{(int)(t % 60f):00}";
        if (goKillsText) goKillsText.text = $"Abatidos: {GameManager.Instance.EnemiesKilled:N0}";
        if (goBestScoreText) goBestScoreText.text = $"Recorde: {SaveSystem.Instance?.Data?.BestScore:N0}";
    }

    // --- Boss ---

    public void ShowBossWarning()
    {
        if (bossWarningPanel) bossWarningPanel.SetActive(true);
        bossWarningAnimator?.SetTrigger("Show");
        StartCoroutine(HideAfter(bossWarningPanel, 3f));
    }

    public void ShowBossHP(string bossName, float hpPercent)
    {
        if (bossHPContainer) bossHPContainer.SetActive(true);
        if (bossNameText) bossNameText.text = bossName;
        if (bossHPBar) bossHPBar.value = hpPercent;
    }

    public void HideBossHP()
    {
        if (bossHPContainer) bossHPContainer.SetActive(false);
    }

    // --- Escudo ---

    public void ShowShieldIndicator(bool active)
    {
        if (shieldIcon) shieldIcon.SetActive(active);
    }

    // --- Utilitários ---

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel) panel.SetActive(active);
    }

    private IEnumerator HideAfter(GameObject obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (obj) obj.SetActive(false);
    }
}
