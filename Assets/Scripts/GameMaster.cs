using Unity.VisualScripting;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] private UIManager uimanager;
    public static GameMaster Instance { get; private set; }
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


    private void Awake()
    {
        // Simple Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persists across scenes
    }

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

        setDisplay(_chits, _mult, _score);


    }

    private void setDisplay(int? cHits = null, float? multi = null, float? score = null)
    {
        if (cHits.HasValue)
        {
            uimanager.SetComboDisplay(cHits.Value);
            uimanager.initComboSlider(comboDuration);
        }

        else uimanager.DisableComboDisplay();

        if (multi.HasValue) uimanager.SetMultiplierDisplay(multi.Value);
        else uimanager.DisableMultiplierDisplay();

        if (score.HasValue) uimanager.SetScoreDisplay(score.Value);

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
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