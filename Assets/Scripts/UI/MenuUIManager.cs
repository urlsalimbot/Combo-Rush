using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Leaderboard leaderboard;
    [SerializeField] private GameObject leaderboardPanel;

    private void Awake()
    {
        // Ensure StateMaster exists
        var stateMaster = StateMaster.Instance;
        if (stateMaster == null)
        {
            Debug.LogError("[MenuUIManager] StateMaster not found in scene!");
        }
    }

    private void OnEnable()
    {
        StateMaster.Instance.OnStateChanged += Setup;
    }

    private void OnDisable()
    {
        if (StateMaster.Instance != null)
        {
            StateMaster.Instance.OnStateChanged -= Setup;
        }
    }

    private void Setup(GameState newState)
    {
        switch (newState)
        {
            case GameState.Unstarted:
                ConfigureMenuForUnstarted();
                break;

            case GameState.Paused:
                ConfigureMenuForPaused();
                break;

            case GameState.Playing:
                HideMenu();
                break;

            default:
                HideMenu();
                break;
        }
    }

    private void ConfigureMenuForUnstarted()
    {
        startButton.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);
    }

    private void ConfigureMenuForPaused()
    {
        Debug.Log("Configuring Menu for Paused State");
        startButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(true);
        mainMenuButton.gameObject.SetActive(true);
    }

    private void HideMenu()
    {
        gameObject.SetActive(false);
    }

    public void OnStartClicked()
    {
        if (StateMaster.Instance != null)
        {
            StateMaster.Instance.StartGame();
        }
    }

    public void OnResumeClicked()
    {
        if (StateMaster.Instance != null)
        {
            StateMaster.Instance.Resume();
        }
    }

    public void OnLeaderboardClicked()
    {
        if (leaderboardPanel.activeSelf)
        {
            leaderboardPanel.SetActive(false);
            return;
        }

        if (leaderboard == null)
        {
            Debug.LogError("[MenuUIManager] Leaderboard reference not assigned!");
            return;
        }

        Debug.Log("[MenuUIManager] Refreshing leaderboard display...");
        leaderboard.RefreshDisplay();
        leaderboardPanel.SetActive(true);
    }

    public void OnMainMenuClicked()
    {
        Debug.Log("[MenuUIManager] OnMainMenuClicked - Reloading scene...");
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
