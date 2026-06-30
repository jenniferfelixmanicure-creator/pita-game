using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>Menu principal com animações, botões e integração com todos os sistemas.</summary>
public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }

    [Header("Painéis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject seasonPassPanel;
    [SerializeField] private GameObject achievementsPanel;
    [SerializeField] private GameObject dailyRewardPanel;
    [SerializeField] private GameObject missionsPanel;

    [Header("Header Info")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI gemText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private Slider seasonProgressBar;

    [Header("Botões Principais")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button rankingButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button seasonPassButton;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private Button missionsButton;

    [Header("Daily Reward")]
    [SerializeField] private GameObject dailyRewardBadge;

    [Header("Animações")]
    [SerializeField] private Animator logoAnimator;
    [SerializeField] private ParticleSystem menuParticles;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SetupButtons();
        RefreshUI();
        CheckDailyReward();
        menuParticles?.Play();
        logoAnimator?.SetTrigger("Pulse");

        AudioManager.Instance?.PlayMusic("menu_theme");
    }

    private void SetupButtons()
    {
        playButton?.onClick.AddListener(OnPlay);
        shopButton?.onClick.AddListener(() => ShowPanel(shopPanel));
        rankingButton?.onClick.AddListener(() => ShowPanel(rankingPanel));
        settingsButton?.onClick.AddListener(() => ShowPanel(settingsPanel));
        seasonPassButton?.onClick.AddListener(() => ShowPanel(seasonPassPanel));
        achievementsButton?.onClick.AddListener(() => ShowPanel(achievementsPanel));
        missionsButton?.onClick.AddListener(() => ShowPanel(missionsPanel));
    }

    public void RefreshUI()
    {
        var data = SaveSystem.Instance?.Data;
        if (data == null) return;

        if (coinText) coinText.text = data.coins.ToString("N0");
        if (gemText) gemText.text = data.gems.ToString("N0");
        if (playerNameText) playerNameText.text = data.playerName;
        if (playerLevelText) playerLevelText.text = $"Nv {data.permanentLevel}";

        if (seasonProgressBar && SeasonPass.Instance != null)
            seasonProgressBar.value = SeasonPass.Instance.GetProgressPercent();
    }

    private void CheckDailyReward()
    {
        bool can = DailyRewardSystem.Instance?.CanClaimToday() ?? false;
        dailyRewardBadge?.SetActive(can);
        if (can) ShowPanel(dailyRewardPanel);
    }

    public void OnPlay()
    {
        ShowPanel(characterSelectPanel);
        AudioManager.Instance?.Play("button_click");
    }

    public void StartGame(string characterId)
    {
        if (string.IsNullOrEmpty(characterId)) characterId = "dark_knight";
        GameManager.SelectedCharacterId = characterId;
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        AudioManager.Instance?.Play("button_click");
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene("Game");
    }

    private void ShowPanel(GameObject panel)
    {
        foreach (var p in new[] { mainPanel, characterSelectPanel, shopPanel,
            rankingPanel, settingsPanel, seasonPassPanel, achievementsPanel,
            dailyRewardPanel, missionsPanel })
            if (p) p.SetActive(false);

        if (panel) { panel.SetActive(true); AudioManager.Instance?.Play("panel_open"); }
        else if (mainPanel) mainPanel.SetActive(true);
    }

    public void BackToMain() => ShowPanel(mainPanel);
}
