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
    }
    private void OnDisable()
    {
        BulletTarget.OnTargetHit -= TargetHit;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        currGameDuration = gameDuration;
        currComboDuration = comboDuration;
        _score = 0;
        setDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        if (StateMaster.Instance.CurrentState != GameState.Playing) return;

        if (currGameDuration <= 0)
        {
            Debug.Log("Game Over!");
            leaderboard.AddEntry(StateMaster.Instance.PlayerName, (int)_score); // Example: Add to leaderboard
            StateMaster.Instance.SetState(GameState.GameOver);
            return;
        }

        if (currGameDuration > 0)
        {
            currGameDuration -= Time.fixedUnscaledDeltaTime;
            uimanager.SetTimeDisplay(currGameDuration);
            Debug.Log($"Time Remaining: {currGameDuration}");
        }

        if (isComboing && currComboDuration > 0)
        {
            currComboDuration -= Time.deltaTime;
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
}