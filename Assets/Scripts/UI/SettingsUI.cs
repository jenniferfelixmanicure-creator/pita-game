using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Tela de configurações: som, música, gráficos, vibração, idioma, conta.</summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Áudio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle vibrationToggle;

    [Header("Gráficos")]
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private Toggle fpsCounterToggle;
    [SerializeField] private Toggle screenShakeToggle;

    [Header("Jogo")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button saveNameButton;
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button deleteAccountButton;

    [Header("Info")]
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TextMeshProUGUI buildText;

    private void OnEnable()
    {
        LoadSettings();
        if (versionText) versionText.text = $"v{Application.version}";
        if (buildText) buildText.text = $"Build {Application.buildGUID.Substring(0, 8).ToUpper()}";
    }

    private void Start()
    {
        musicSlider?.onValueChanged.AddListener(v => { AudioManager.Instance?.SetMusicVolume(v); PlayerPrefs.SetFloat("music_vol", v); });
        sfxSlider?.onValueChanged.AddListener(v => { AudioManager.Instance?.SetSFXVolume(v); PlayerPrefs.SetFloat("sfx_vol", v); });
        vibrationToggle?.onValueChanged.AddListener(v => PlayerPrefs.SetInt("vibration", v ? 1 : 0));
        qualitySlider?.onValueChanged.AddListener(v => QualitySettings.SetQualityLevel((int)v, true));
        screenShakeToggle?.onValueChanged.AddListener(v => PlayerPrefs.SetInt("screenshake", v ? 1 : 0));
        fpsCounterToggle?.onValueChanged.AddListener(v => PlayerPrefs.SetInt("fps_counter", v ? 1 : 0));

        saveNameButton?.onClick.AddListener(SavePlayerName);
        resetProgressButton?.onClick.AddListener(ConfirmReset);
        deleteAccountButton?.onClick.AddListener(ConfirmDelete);
    }

    private void LoadSettings()
    {
        if (musicSlider) musicSlider.value = PlayerPrefs.GetFloat("music_vol", 0.8f);
        if (sfxSlider) sfxSlider.value = PlayerPrefs.GetFloat("sfx_vol", 1f);
        if (vibrationToggle) vibrationToggle.isOn = PlayerPrefs.GetInt("vibration", 1) == 1;
        if (qualitySlider) qualitySlider.value = QualitySettings.GetQualityLevel();
        if (screenShakeToggle) screenShakeToggle.isOn = PlayerPrefs.GetInt("screenshake", 1) == 1;
        if (fpsCounterToggle) fpsCounterToggle.isOn = PlayerPrefs.GetInt("fps_counter", 0) == 1;
        if (playerNameInput && SaveSystem.Instance?.Data != null)
            playerNameInput.text = SaveSystem.Instance.Data.playerName;
    }

    private void SavePlayerName()
    {
        string name = playerNameInput?.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(name) || name.Length > 20) return;
        if (SaveSystem.Instance?.Data != null)
        {
            SaveSystem.Instance.Data.playerName = name;
            SaveSystem.Instance.Save();
            UIManager.Instance?.ShowMessage("Nome salvo!");
        }
        PlayerPrefs.Save();
    }

    private void ConfirmReset()
    {
        // Em produção: abrir dialog de confirmação
        UIManager.Instance?.ShowConfirmDialog("Zerar todo o progresso?", () =>
        {
            SaveSystem.Instance?.ResetProgress();
            UIManager.Instance?.ShowMessage("Progresso zerado.");
        });
    }

    private void ConfirmDelete()
    {
        UIManager.Instance?.ShowConfirmDialog("Excluir conta Firebase?", () =>
        {
            FirebaseManager.Instance?.DeleteAccount();
        });
    }
}
