using Unity.VisualScripting;
using UnityEngine;

public class ComboGenerator : MonoBehaviour
{
    [SerializeField] private UIManager uimanager;
    public static ComboGenerator Instance { get; private set; }
    public int Score { get; private set; }


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
        BulletTarget.OnTargetHit += IncrementScore;
    }
    private void OnDisable()
    {
        BulletTarget.OnTargetHit -= IncrementScore;
    }

    private void IncrementScore(int addScore)
    {
        Debug.Log("Hit Scored, incrementing Score");
        Score += addScore;

        if (uimanager != null)
            uimanager.SetScoreDisplay(Score);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
