using UnityEngine;
using Singletons;
using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;

public enum GameState { Loading, Unstarted, Playing, Paused, GameOver, CountingDown }

/// <summary>
/// Central state machine manager for game flow control.
/// Handles state transitions, UI visibility, time scale, and cursor management.
/// This is a scene-specific singleton (destroyed on scene unload).
/// </summary>
public class StateMaster : Singleton<StateMaster>
{
    protected override bool PersistAcrossScenes => false;

    private const float DefaultFixedDeltaTime = 0.02f;
    private const int FlashErrorIterations = 3;
    private const float FlashErrorDuration = 0.1f;

    // Events for state changes - allows decoupled reactions from other systems
    public event Action<GameState> OnStateChanged;
    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameOver;

    [Header("UI Panels")]
    [SerializeField] private MenuUIManager menuPanel;
    [SerializeField] private HUDUIManager gameplayHUD;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("References")]
    [SerializeField] private CinemachineCamera mainMenuCamera;
    [SerializeField] private CinemachineBrain mainCamera;

    [Header("Settings")]
    [SerializeField] private int startCountdownValue = 3;

    public GameState CurrentState { get; private set; }
    public string PlayerName { get; private set; }
    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsGameOver => CurrentState == GameState.GameOver;
    public bool IsCountingDown => CurrentState == GameState.CountingDown;

    private bool _isTransitioning;

    private void Start()
    {
        Debug.Log("[StateMaster] Start() called - Initializing...");
        
        // Force reset time and cursor before initializing
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SetState(GameState.Unstarted);
        InitializeUI();
        
        Debug.Log("[StateMaster] Initialization complete. Current state: " + CurrentState);
    }

    private void InitializeUI()
    {
        OnStateChanged?.Invoke(CurrentState);
        if (menuPanel != null) menuPanel.gameObject.SetActive(true);
        if (gameplayHUD != null) gameplayHUD.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Validates and initiates the game start sequence.
    /// </summary>
    public void StartGame()
    {
        if (CurrentState != GameState.Unstarted)
        {
            Debug.LogWarning($"Cannot start game from state: {CurrentState}");
            return;
        }

        if (nameInputField == null || string.IsNullOrWhiteSpace(nameInputField.text))
        {
            StopCoroutine(nameof(FlashErrorRoutine));
            StartCoroutine(FlashErrorRoutine());
            return;
        }

        PlayerName = nameInputField.text.Trim();
        nameInputField.gameObject.SetActive(false);

        if (mainMenuCamera != null)
        {
            mainMenuCamera.gameObject.SetActive(false);
        }
        BeginCountdownSequence();
    }

    /// <summary>
    /// Sets the game state with proper transition handling and validation.
    /// </summary>
    public void SetState(GameState newState)
    {
        if (newState == CurrentState)
        {
            Debug.LogWarning($"Already in state: {newState}");
            return;
        }

        if (!IsValidTransition(CurrentState, newState))
        {
            Debug.LogError($"Invalid state transition: {CurrentState} -> {newState}");
            return;
        }

        GameState previousState = CurrentState;
        CurrentState = newState;
        _isTransitioning = false;

        ApplyStateEffects(newState, previousState);
        OnStateChanged?.Invoke(newState);

        Debug.Log($"Game State: {previousState} -> {newState}");
    }

    /// <summary>
    /// Validates if a state transition is allowed.
    /// </summary>
    private bool IsValidTransition(GameState from, GameState to)
    {
        switch (from)
        {
            case GameState.Unstarted:
                return to == GameState.Playing || to == GameState.CountingDown;

            case GameState.Paused:
                return to == GameState.Playing || to == GameState.GameOver || to == GameState.CountingDown;

            case GameState.GameOver:
                return to == GameState.Unstarted;

            case GameState.Playing:
                return to == GameState.Paused || to == GameState.GameOver || to == GameState.CountingDown;

            default:
                return true;
        }
    }

    /// <summary>
    /// Applies effects based on the new state (time scale, cursor, UI visibility).
    /// </summary>
    private void ApplyStateEffects(GameState newState, GameState previousState)
    {
        Debug.Log($"Applying state effects for: {newState}");

        // 1. Time Scale - only running during Playing state
        Time.timeScale = (newState == GameState.Playing) ? 1f : 0f;

        // 2. Cursor - visible when not playing
        bool cursorVisible = newState != GameState.Playing;
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;

        // 3. UI Visibility
        if (menuPanel != null)
        {
            menuPanel.gameObject.SetActive(newState == GameState.Unstarted || newState == GameState.Paused);
        }
        if (gameplayHUD != null)
        {
            gameplayHUD.gameObject.SetActive(newState == GameState.Playing || newState == GameState.CountingDown);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(newState == GameState.GameOver);
        }

        // 4. Camera handling
        switch (newState)
        {
            case GameState.Unstarted:
                if (mainMenuCamera != null)
                {
                    mainMenuCamera.gameObject.SetActive(true);
                }
                break;

            case GameState.CountingDown:
            case GameState.Playing:
            case GameState.GameOver:
                if (mainMenuCamera != null)
                {
                    mainMenuCamera.gameObject.SetActive(false);
                }
                break;
        }

        // 5. State-specific setup
        if (newState == GameState.Playing && previousState == GameState.Paused)
        {
            OnGameResumed?.Invoke();
        }
        else if (newState == GameState.GameOver)
        {
            OnGameOver?.Invoke();
        }
    }

    /// <summary>
    /// Begins the countdown sequence before gameplay starts.
    /// </summary>
    private void BeginCountdownSequence()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        StartCoroutine(StartCountdownRoutine(CurrentState));
    }

    private IEnumerator StartCountdownRoutine(GameState previousState)
    {
        // Manually handle CountingDown state (bypass SetState to control camera blend timing)
        CurrentState = GameState.CountingDown;
        Time.timeScale = 0f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (menuPanel != null) menuPanel.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(true);
        if (gameplayHUD != null) gameplayHUD.gameObject.SetActive(true);

        // Only handle camera when transitioning from Unstarted (not from Paused)
        if (previousState == GameState.Unstarted)
        {
            if (mainMenuCamera != null)
            {
                mainMenuCamera.Priority = 0;
                mainMenuCamera.gameObject.SetActive(false);
            }

            if (mainCamera != null)
            {
                CinemachineBlendDefinition originalBlend = mainCamera.DefaultBlend;
                mainCamera.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 2.0f);

                OnStateChanged?.Invoke(CurrentState);
                Debug.Log($"Game State: {previousState} -> {GameState.CountingDown}");

                float timer = startCountdownValue;
                while (timer > 0)
                {
                    if (countdownText != null)
                    {
                        countdownText.text = Mathf.CeilToInt(timer).ToString();
                    }
                    yield return new WaitForSecondsRealtime(1f);
                    timer--;
                }

                if (countdownText != null) countdownText.gameObject.SetActive(false);
                mainCamera.DefaultBlend = originalBlend;
            }
        }
        else
        {
            // Transitioning from Paused - simpler countdown without camera changes
            OnStateChanged?.Invoke(CurrentState);
            Debug.Log($"Game State: {previousState} -> {GameState.CountingDown}");

            float timer = startCountdownValue;
            while (timer > 0)
            {
                if (countdownText != null)
                {
                    countdownText.text = Mathf.CeilToInt(timer).ToString();
                }
                yield return new WaitForSecondsRealtime(1f);
                timer--;
            }

            if (countdownText != null) countdownText.gameObject.SetActive(false);
        }

        // Transition to Playing state
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        gameplayHUD.gameObject.SetActive(true);
        _isTransitioning = false;

        OnStateChanged?.Invoke(CurrentState);

        // Only invoke OnGameStarted when starting fresh (not when resuming)
        if (previousState == GameState.Unstarted)
        {
            OnGameStarted?.Invoke();
        }
        else
        {
            OnGameResumed?.Invoke();
        }

        Debug.Log($"Game State: {GameState.CountingDown} -> {GameState.Playing}");
    }

    private IEnumerator FlashErrorRoutine()
    {
        if (errorText == null) yield break;

        errorText.gameObject.SetActive(true);
        errorText.text = "ENTER NAME!";

        for (int i = 0; i < FlashErrorIterations; i++)
        {
            errorText.color = Color.red;
            yield return new WaitForSecondsRealtime(FlashErrorDuration);
            errorText.color = Color.white;
            yield return new WaitForSecondsRealtime(FlashErrorDuration);
        }

        errorText.color = Color.red;
        errorText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Resumes from paused state.
    /// </summary>
    public void Resume()
    {
        if (CurrentState != GameState.Paused)
        {
            Debug.LogWarning($"Cannot resume from state: {CurrentState}");
            return;
        }
        BeginCountdownSequence();
    }

    /// <summary>
    /// Pauses the game from playing state, or resumes if already paused.
    /// </summary>
    public void Pause()
    {
        if (CurrentState == GameState.Paused)
        {
            Resume();
        }
        else if (CurrentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
    }

    /// <summary>
    /// Triggers game over sequence.
    /// </summary>
    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing && CurrentState != GameState.Paused)
        {
            Debug.LogWarning($"Cannot trigger game over from state: {CurrentState}");
            return;
        }
        SetState(GameState.GameOver);
    }

    /// <summary>
    /// Resets the game to unstarted state for a new round.
    /// </summary>
    public void ResetGame()
    {
        if (CurrentState != GameState.GameOver && CurrentState != GameState.Unstarted)
        {
            Debug.LogWarning($"Cannot reset from state: {CurrentState}. Trigger GameOver first.");
            return;
        }

        PlayerName = null;

        InitializeUI();
        if (nameInputField != null)
        {
            nameInputField.gameObject.SetActive(true);
            nameInputField.text = string.Empty;
        }

        SetState(GameState.Unstarted);
    }

    public void Quit() => Application.Quit();
}
