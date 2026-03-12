using UnityEngine;
using Singletons;
using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;

public enum GameState { Unstarted, Playing, Paused, GameOver, CountingDown }

/// <summary>
/// Central state machine manager for game flow control.
/// Handles state transitions, UI visibility, time scale, and cursor management.
/// This is a scene-specific singleton (does not persist across scene loads).
/// </summary>
public class StateMaster : Singleton<StateMaster>
{
    // Events for state changes - allows decoupled reactions from other systems
    public event Action<GameState> OnStateChanged;
    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameOver;

    protected override bool PersistAcrossScenes => true;

    [Header("UI Panels")]
    [SerializeField] private MenuUIManager MenuPanel;
    [SerializeField] private HUDUIManager gameplayHUD;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("References")]
    [SerializeField] private CinemachineCamera mainmenucamera;
    [SerializeField] private CinemachineBrain maincamera;

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
        SetState(GameState.Unstarted);
        InitializeUI();
        ApplyStateEffects(CurrentState, CurrentState); // Ensure initial state effects are applied
    }

    private void InitializeUI()
    {
        MenuPanel.gameObject.SetActive(true);
        MenuPanel.Setup();
        gameplayHUD.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);
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

        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            StopCoroutine(nameof(FlashErrorRoutine));
            StartCoroutine(FlashErrorRoutine());
            return;
        }

        PlayerName = nameInputField.text.Trim();
        nameInputField.gameObject.SetActive(false);

        mainmenucamera.gameObject.SetActive(false);
        // Begin countdown sequence which transitions to Playing
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
        // Allow any transition from Unstarted
        if (from == GameState.Unstarted)
            return to == GameState.Playing || to == GameState.CountingDown;

        // Allow transitions to Paused only from Playing
        if (to == GameState.Paused)
            return from == GameState.Playing;

        // Allow transitions from Paused to Playing or GameOver
        if (from == GameState.Paused)
            return to == GameState.Playing || to == GameState.GameOver;

        // Allow transitions to GameOver from Playing or Paused
        if (to == GameState.GameOver)
            return from == GameState.Playing || from == GameState.Paused;

        // Allow transitions from GameOver to Unstarted (reset)
        if (from == GameState.GameOver)
            return to == GameState.Unstarted;

        // Default: allow transitions from Playing
        if (from == GameState.Playing)
            return to == GameState.Paused || to == GameState.GameOver || to == GameState.CountingDown;

        return true;
    }

    /// <summary>
    /// Applies effects based on the new state (time scale, cursor, UI visibility).
    /// </summary>
    private void ApplyStateEffects(GameState newState, GameState previousState)
    {
        // 1. Time Scale - only running during Playing state
        Time.timeScale = (newState == GameState.Playing) ? 1f : 0f;

        // 2. Cursor - visible when not playing
        bool cursorVisible = newState != GameState.Playing;
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;

        // 3. UI Visibility
        MenuPanel.gameObject.SetActive(newState == GameState.Unstarted || newState == GameState.Paused);
        gameplayHUD.gameObject.SetActive(newState == GameState.Playing || newState == GameState.CountingDown);
        gameOverPanel.SetActive(newState == GameState.GameOver);

        // 4. Camera handling - disable menu camera during countdown and playing
        if (newState == GameState.Unstarted)
        {
            mainmenucamera.gameObject.SetActive(true);
        }
        else if (newState == GameState.CountingDown || newState == GameState.Playing || newState == GameState.GameOver)
        {
            mainmenucamera.gameObject.SetActive(false);
        }

        // 5. State-specific setup
        if (newState == GameState.Unstarted)
        {
            MenuPanel.Setup();
        }
        else if (newState == GameState.Paused)
        {
            MenuPanel.Setup();
            OnGamePaused?.Invoke();
        }
        else if (newState == GameState.Playing && previousState == GameState.Paused)
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

        StartCoroutine(StartCountdownRoutine());
    }

    private IEnumerator StartCountdownRoutine()
    {
        // Manually handle CountingDown state (bypass SetState to control camera blend timing)
        CurrentState = GameState.CountingDown;
        Time.timeScale = 0f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        MenuPanel.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        gameplayHUD.gameObject.SetActive(true);
        
        // Disable menu camera FIRST to allow player camera to blend in
        mainmenucamera.Priority= 0; // Lower priority to allow main camera to take over
        mainmenucamera.gameObject.SetActive(false); // Deactivate menu camera to trigger blend

        // Configure blend for smooth camera transition
        CinemachineBlendDefinition originalBlend = maincamera.DefaultBlend;
        maincamera.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 2.0f);

        OnStateChanged?.Invoke(CurrentState);
        Debug.Log($"Game State: {GameState.Unstarted} -> {GameState.CountingDown}");

        float timer = startCountdownValue;
        while (timer > 0)
        {
            countdownText.text = Mathf.CeilToInt(timer).ToString();
            yield return new WaitForSecondsRealtime(1f);
            timer--;
        }

        countdownText.gameObject.SetActive(false);
        maincamera.DefaultBlend = originalBlend;

        // Transition to Playing state
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        gameplayHUD.gameObject.SetActive(true);
        _isTransitioning = false;

        OnStateChanged?.Invoke(CurrentState);
        OnGameStarted?.Invoke();

        Debug.Log($"Game State: {GameState.CountingDown} -> {GameState.Playing}");
    }

    private IEnumerator FlashErrorRoutine()
    {
        if (errorText == null) yield break;

        errorText.gameObject.SetActive(true);
        errorText.text = "ENTER NAME!";

        for (int i = 0; i < 3; i++)
        {
            errorText.color = Color.red;
            yield return new WaitForSecondsRealtime(0.1f);
            errorText.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
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
        SetState(GameState.Playing);
    }

    /// <summary>
    /// Pauses the game from playing state.
    /// </summary>
    public void Pause()
    {
        if (CurrentState != GameState.Playing)
        {
            Debug.LogWarning($"Cannot pause from state: {CurrentState}");
            return;
        }
        SetState(GameState.Paused);
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

        // Reset all game state
        PlayerName = null;

        // Reset UI
        InitializeUI();
        nameInputField.gameObject.SetActive(true);
        nameInputField.text = string.Empty;

        // Set state to Unstarted (will handle cursor, camera, time scale)
        SetState(GameState.Unstarted);
    }

    public void Quit() => Application.Quit();
}
