using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Gerenciador central do jogo. Singleton que persiste entre cenas.
/// Controla estado do jogo, eventos globais e inicialização dos sistemas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // --- Estado do jogo ---
    public enum GameState { MainMenu, Playing, Paused, GameOver, Victory, Loading }
    public GameState CurrentState { get; private set; }

    // --- Eventos globais ---
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnGameStarted;
    public static event Action OnGameOver;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    // --- Configurações ---
    [Header("Configurações Gerais")]
    [SerializeField] private float targetFrameRate = 60f;
    [SerializeField] private bool enableVibration = true;

    // --- Referências dos sistemas ---
    [Header("Sistemas")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private UIManager uiManager;

    // --- Dados da sessão atual ---
    public int CurrentScore { get; private set; }
    public float SessionTime { get; private set; }
    public int EnemiesKilled { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeGame();
    }

    private void InitializeGame()
    {
        Application.targetFrameRate = (int)targetFrameRate;
        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Carregar dados salvos
        SaveSystem.Instance?.Load();

        // Estado inicial
        ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            SessionTime += Time.deltaTime;
        }
    }

    // --- Controle de estado ---

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                OnGamePaused?.Invoke();
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                HandleGameOver();
                break;
        }
    }

    public void StartGame(int characterIndex = 0)
    {
        ResetSession();
        ChangeState(GameState.Loading);
        SceneManager.LoadScene("GameScene");
        OnGameStarted?.Invoke();
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
            ChangeState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
            OnGameResumed?.Invoke();
        }
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;
        ChangeState(GameState.GameOver);
        OnGameOver?.Invoke();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        ChangeState(GameState.MainMenu);
    }

    // --- Pontuação e stats ---

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        UIManager.Instance?.UpdateScore(CurrentScore);
    }

    public void RegisterEnemyKill()
    {
        EnemiesKilled++;
        UIManager.Instance?.UpdateKillCount(EnemiesKilled);
    }

    private void HandleGameOver()
    {
        // Salvar recordes
        var save = SaveSystem.Instance?.Data;
        if (save != null)
        {
            if (CurrentScore > save.BestScore) save.BestScore = CurrentScore;
            if (SessionTime > save.BestSurvivalTime) save.BestSurvivalTime = SessionTime;
            if (EnemiesKilled > save.MostKillsInRun) save.MostKillsInRun = EnemiesKilled;
            SaveSystem.Instance.Save();
        }

        if (enableVibration) Handheld.Vibrate();
    }

    private void ResetSession()
    {
        CurrentScore = 0;
        SessionTime = 0f;
        EnemiesKilled = 0;
    }

    // --- Utilitários ---

    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsPaused => CurrentState == GameState.Paused;

    public void Vibrate()
    {
        if (enableVibration) Handheld.Vibrate();
    }
}
