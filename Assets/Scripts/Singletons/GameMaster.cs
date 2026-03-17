using System;
using UnityEngine;
using Singletons;

public class GameMaster : Singleton<GameMaster>
{
    protected override bool PersistAcrossScenes => false;

    [Header("References")]
    [SerializeField] private HUDUIManager uiManager;
    [SerializeField] private Leaderboard leaderboard;
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Game Parameters")]
    [SerializeField] private float gameDuration = 90f;

    [Header("Combo Parameters")]
    [SerializeField] private float comboDuration = 7f;
    [SerializeField] private float comboMultiplierIncrement = 0.25f;
    [SerializeField] private int comboThreshold = 1;

    private float _currentGameDuration;
    private float _currentComboDuration;
    private float _score;
    private float _currentMultiplier;
    private int _comboHits;
    private bool _isComboing;
    private int _incomingScore;

    private void OnEnable()
    {
        Debug.Log("[GameMaster] OnEnable() called - Subscribing to events");
        
        BulletTarget.OnTargetHit += HandleTargetHit;
        ThirdPersonShooterController.OnTargetMiss += HandleTargetMiss;
        
        StateMaster.Instance.OnGameStarted += OnGameStarted;
        StateMaster.Instance.OnGameResumed += OnGameResumed;
        StateMaster.Instance.OnGameOver += OnGameOver;
    }



    private void OnDisable()
    {
        BulletTarget.OnTargetHit -= HandleTargetHit;
        ThirdPersonShooterController.OnTargetMiss += HandleTargetMiss;
        
        if (StateMaster.Instance != null)
        {
            StateMaster.Instance.OnGameStarted -= OnGameStarted;
            StateMaster.Instance.OnGameResumed -= OnGameResumed;
            StateMaster.Instance.OnGameOver -= OnGameOver;
        }
    }

    private void OnGameStarted()
    {
        _currentGameDuration = gameDuration;
        _currentComboDuration = comboDuration;
        _score = 0;
        _currentMultiplier = 1f;
        _comboHits = 0;
        _isComboing = false;
        UpdateDisplay();
        Debug.Log("Game Started - Timers Reset");
    }

    private void OnGameResumed()
    {
        Debug.Log("Game Resumed - Timers Continue");    
    }

    private void OnGameOver()
    {
        Debug.Log("Game Over - Submitting Score");
        gameOverManager.Setup(Mathf.RoundToInt(_score), _comboHits, _currentMultiplier);
    }

        private void HandleTargetMiss()
    {
        _currentComboDuration = 0f;
    }

    private void HandleTargetHit(int addScore)
    {
        Debug.Log("Hit Scored, incrementing Score");
        _incomingScore = addScore;
        _currentComboDuration = comboDuration;
        ProcessComboLogic();
    }

    private void ProcessComboLogic()
    {
        _isComboing = true;
        _comboHits++;

        Debug.Log($"Combo Hits = {_comboHits}");

        if (_comboHits > 1 && _comboHits % comboThreshold == 0)
        {
            _currentMultiplier += comboMultiplierIncrement;
        }

        _score += _incomingScore * _currentMultiplier;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_comboHits > 0)
        {
            uiManager.SetComboDisplay(_comboHits);
            uiManager.InitComboSlider(comboDuration);
        }
        else
        {
            uiManager.DisableComboDisplay();
        }

        uiManager.SetMultiplierDisplay(_currentMultiplier);
        uiManager.SetScoreDisplay(_score);
    }

    private void Update()
    {
        if (StateMaster.Instance == null || !StateMaster.Instance.IsPlaying) return;

        if (_currentGameDuration <= 0)
        {
            Debug.Log("Game Over!");
            StateMaster.Instance.TriggerGameOver();
            return;
        }

        _currentGameDuration -= Time.unscaledDeltaTime;
        uiManager.SetTimeDisplay(_currentGameDuration);

        if (_isComboing && _currentComboDuration > 0)
        {
            _currentComboDuration -= Time.unscaledDeltaTime;
            uiManager.SetComboSlider(_currentComboDuration);
        }

        if (_isComboing && _currentComboDuration <= 0)
        {
            Debug.Log("Combo Over! RESETTING");
            ResetCombo();
        }
    }

    private void ResetCombo()
    {
        _isComboing = false;
        uiManager.DisableComboDisplay();
        uiManager.DisableMultiplierDisplay();
        uiManager.DisableComboSlider();

        _currentComboDuration = comboDuration;
        _comboHits = 0;
        _currentMultiplier = 1f;
    }

    public void SubmitScore()
    {
        int finalScore = Mathf.RoundToInt(_score);
        string playerName = StateMaster.Instance != null ? StateMaster.Instance.PlayerName : "Unknown";
        
        if (leaderboard == null)
        {
            Debug.LogError("[GameMaster] Leaderboard reference not assigned! Cannot submit score.");
            return;
        }
        
        Debug.Log($"[GameMaster] Submitting score: {finalScore} for player: {playerName}");
        leaderboard.AddEntry(playerName, finalScore);
        Debug.Log($"[GameMaster] Score submitted successfully");
    }
}
