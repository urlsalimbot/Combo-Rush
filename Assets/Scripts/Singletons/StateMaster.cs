using UnityEngine;
using Singletons;
using System;

public enum GameState { Loading, Unstarted, Playing, Paused, GameOver }

/// <summary>
/// Central state machine manager for game flow control.
/// Handles state transitions, time scale, and cursor management.
/// This is a scene-specific singleton (destroyed on scene unload).
/// </summary>
public class StateMaster : Singleton<StateMaster>
{
    protected override bool PersistAcrossScenes => false;

    // Events for state changes - allows decoupled reactions from other systems
    public event Action<GameState> OnStateChanged;
    public event Action OnGameStarted;
    public event Action OnGameResumed;
    public event Action OnGameOver;
    public event Action OnCountdownStarted;

    public GameState CurrentState { get; private set; }
    public string PlayerName { get; private set; }
    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsGameOver => CurrentState == GameState.GameOver;

    private void Start()
    {
        Debug.Log("[StateMaster] Start() called - Initializing...");

        // Force reset time and cursor before initializing
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SetState(GameState.Unstarted);

        Debug.Log("[StateMaster] Initialization complete. Current state: " + CurrentState);
    }

    /// <summary>
    /// Validates player name and initiates the game start sequence.
    /// UI should validate name before calling this.
    /// </summary>
    public void StartGame(string playerName)
    {
        if (CurrentState != GameState.Unstarted)
        {
            Debug.LogWarning($"Cannot start game from state: {CurrentState}");
            return;
        }

        PlayerName = playerName.Trim();
        OnCountdownStarted?.Invoke();
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
                return to == GameState.Playing;

            case GameState.Paused:
                return to == GameState.Playing || to == GameState.GameOver;

            case GameState.GameOver:
                return to == GameState.Unstarted;

            case GameState.Playing:
                return to == GameState.Paused || to == GameState.GameOver;

            default:
                return true;
        }
    }

    /// <summary>
    /// Applies effects based on the new state (time scale, cursor).
    /// UI visibility is handled by individual UI components via events.
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

        // 3. State-specific setup
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
    /// Resumes from paused state.
    /// </summary>
    public void Resume()
    {
        if (CurrentState != GameState.Paused)
        {
            Debug.LogWarning($"Cannot resume from state: {CurrentState}");
            return;
        }
        OnCountdownStarted?.Invoke();
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
        SetState(GameState.Unstarted);
    }

    public void Quit() => Application.Quit();

    /// <summary>
    /// Called by MenuUIManager when countdown completes.
    /// </summary>
    public void OnCountdownFinished(bool wasFromPause)
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        OnStateChanged?.Invoke(CurrentState);

        if (wasFromPause)
        {
            OnGameResumed?.Invoke();
        }
        else
        {
            OnGameStarted?.Invoke();
        }

        Debug.Log($"Game State: CountingDown -> {GameState.Playing}");
    }
}
