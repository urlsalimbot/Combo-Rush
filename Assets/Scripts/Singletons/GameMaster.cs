using System;
using UnityEngine;
using Singletons;

public class GameMaster : Singleton<GameMaster>
{

    [Header("References")]
    [SerializeField] private HUDUIManager uimanager;
    [SerializeField] private Leaderboard leaderboard;

    [Header("Game Parameters")]
    [SerializeField] private float gameDuration = 90f;

    private float currGameDuration;
    public float _score { get; private set; }

    [Header("Combo Parameters")]
    [SerializeField] private float comboDuration = 7f;
    [SerializeField] private float comboMultiplicator = 0.25f;
    [SerializeField] private int comboThreshold = 1;
    private bool isComboing = false;

    private int comboHits = 0;
    private float currMultiplicator = 1.0f;

    private float currComboDuration;
    private int incomingScore;


    private void OnEnable()
    {
        BulletTarget.OnTargetHit += TargetHit;
        StateMaster.Instance.OnGameStarted += OnGameStarted;
        StateMaster.Instance.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        BulletTarget.OnTargetHit -= TargetHit;
        StateMaster.Instance.OnGameStarted -= OnGameStarted;
        StateMaster.Instance.OnGameOver -= OnGameOver;
    }

    private void OnGameStarted()
    {
        currGameDuration = gameDuration;
        currComboDuration = comboDuration;
        _score = 0;
        setDisplay();
        Debug.Log("Game Started - Timers Reset");
    }

    private void OnGameOver()
    {
        Debug.Log("Game Over - Submitting Score");
        // Calculate total combos and final multiplier for submission
        int totalCombos = comboHits;
        float finalMultiplier = currMultiplicator;
        Instance.SubmitScore((int)_score, totalCombos, finalMultiplier);
    }

    private void TargetHit(int addScore)
    {
        Debug.Log("Hit Scored, incrementing Score");
        incomingScore = addScore;
        currComboDuration = comboDuration;
        GameLogic();

    }

    private void GameLogic()
    {
        isComboing = true;

        comboHits += 1;
        Debug.Log($"Combo Hits = {comboHits}");

        int? _chits = null;
        float? _mult = null;

        if (comboHits > 1)
        {
            _chits = comboHits;
            if (comboHits % comboThreshold == 0) currMultiplicator += comboMultiplicator;
        }
        ;

        _mult = currMultiplicator;
        _score += incomingScore * _mult.Value;

        setDisplay(_chits, _mult);


    }

    private void setDisplay(int? cHits = null, float? multi = null)
    {
        if (cHits.HasValue)
        {
            uimanager.SetComboDisplay(cHits.Value);
            uimanager.initComboSlider(comboDuration);
        }

        else uimanager.DisableComboDisplay();

        if (multi.HasValue) uimanager.SetMultiplierDisplay(multi.Value);
        else uimanager.DisableMultiplierDisplay();

        uimanager.SetScoreDisplay(_score);

    }

    // Update is called once per frame
    void Update()
    {
        if (!StateMaster.Instance.IsPlaying) return;

        if (currGameDuration <= 0)
        {
            Debug.Log("Game Over!");
            StateMaster.Instance.TriggerGameOver();
            return;
        }

        if (currGameDuration > 0)
        {
            currGameDuration -= Time.unscaledDeltaTime;
            uimanager.SetTimeDisplay(currGameDuration);
            Debug.Log($"Time Remaining: {currGameDuration}");
        }

        if (isComboing && currComboDuration > 0)
        {
            currComboDuration -= Time.unscaledDeltaTime;
            uimanager.SetComboSlider(currComboDuration);
        }

        if (isComboing && currComboDuration <= 0)
        {
            Debug.Log($"Combo Over! RESETTING");
            isComboing = false;
            setDisplay(null, null);
            uimanager.DisableComboSlider();
            currComboDuration = comboDuration;
            comboHits = 0;
            currMultiplicator = 1;
        }
    }

    public void SubmitScore(int finalScore, int totalCombos, float finalMultiplier)
    {
        leaderboard.AddEntry(StateMaster.Instance.PlayerName, (int)_score);
        Debug.Log($"Submitting Score: {finalScore} with Combos: {totalCombos} and Multiplier: {finalMultiplier} for Player: {StateMaster.Instance.PlayerName}");
    }
}