using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.Cinemachine;

public class MenuUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Leaderboard leaderboard;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;

    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private GameObject leaderboardPanel;

    [Header("Camera References")]
    [SerializeField] private CinemachineCamera mainMenuCamera;
    [SerializeField] private CinemachineBrain mainCamera;

    private bool _isTransitioning;
    private int _countdownValue = 3;

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
        StateMaster.Instance.OnCountdownStarted += StartCountdown;
    }

    private void OnDisable()
    {
        if (StateMaster.Instance != null)
        {
            StateMaster.Instance.OnStateChanged -= Setup;
            StateMaster.Instance.OnCountdownStarted -= StartCountdown;
        }
    }

    private void Setup(GameState newState)
    {
        Debug.Log($"Menu UI Manager: {newState}");
        switch (newState)
        {
            case GameState.Unstarted:
                ConfigureMenuForUnstarted();
                break;

            case GameState.Paused:
                ConfigureMenuForPaused();
                break;

            case GameState.Playing:
                Debug.Log("Hiding Menu");
                HideMenu();
                break;

            case GameState.GameOver:
                ShowGameOver();
                break;

            default:
                HideMenu();
                break;
        }
    }

    private void ConfigureMenuForUnstarted()
    {
        Debug.Log("Configuring menu for Unstarted state");
        mainMenuPanel.SetActive(true);
        startButton.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);

        if (nameInputField != null)
        {
            nameInputField.gameObject.SetActive(true);
            nameInputField.text = string.Empty;
        }
        if (gameplayHUD != null) gameplayHUD.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void ConfigureMenuForPaused()
    {
        mainMenuPanel.SetActive(true);
        startButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(true);
        mainMenuButton.gameObject.SetActive(true);

        if (gameplayHUD != null) gameplayHUD.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameplayHUD != null) gameplayHUD.gameObject.SetActive(false);
    }

    private void HideMenu()
    {
        mainMenuPanel.SetActive(false);
        leaderboardPanel.SetActive(false);
    }

    private void StartCountdown()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        Debug.Log("UI MANAGER Starting Countdown...");

        bool isFromPause = StateMaster.Instance.IsPaused;
        gameplayHUD.SetActive(true);
        countdownText.gameObject.SetActive(true);
        StartCoroutine(CountdownRoutine(isFromPause));
    }

    private IEnumerator CountdownRoutine(bool isFromPause)
    {
        // Hide menu and show countdown
        mainMenuPanel.SetActive(false);
        countdownText.gameObject.SetActive(true);
        gameplayHUD.gameObject.SetActive(true);

        // Handle camera when starting fresh (not from pause)
        CinemachineBlendDefinition originalBlend = default;
        bool shouldRestoreBlend = false;

        if (!isFromPause)
        {

            mainMenuCamera.Priority = 0;
            mainMenuCamera.gameObject.SetActive(false);


            originalBlend = mainCamera.DefaultBlend;
            mainCamera.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 2.0f);
            shouldRestoreBlend = true;

        }

        // Countdown
        float timer = _countdownValue;
        while (timer > 0)
        {

            countdownText.text = Mathf.CeilToInt(timer).ToString();
            yield return new WaitForSecondsRealtime(1f);
            timer--;
        }

        // Cleanup
        countdownText.gameObject.SetActive(false);

        if (shouldRestoreBlend && mainCamera != null)
        {
            mainCamera.DefaultBlend = originalBlend;
        }

        _isTransitioning = false;

        // Notify StateMaster that countdown is complete
        StateMaster.Instance.OnCountdownFinished(isFromPause);
    }

    public void OnStartClicked()
    {
        if (StateMaster.Instance == null) return;

        if (nameInputField == null || string.IsNullOrWhiteSpace(nameInputField.text))
        {
            StopCoroutine(nameof(FlashErrorRoutine));
            StartCoroutine(FlashErrorRoutine());
            return;
        }
        nameInputPanel.SetActive(false);


        StateMaster.Instance.StartGame(nameInputField.text);
    }

    public void onEnterNameClicked()
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
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

    private IEnumerator FlashErrorRoutine()
    {
        if (errorText == null) yield break;

        const int flashErrorIterations = 3;
        const float flashErrorDuration = 0.1f;

        errorText.gameObject.SetActive(true);
        errorText.text = "ENTER NAME!";

        for (int i = 0; i < flashErrorIterations; i++)
        {
            errorText.color = Color.red;
            yield return new WaitForSecondsRealtime(flashErrorDuration);
            errorText.color = Color.white;
            yield return new WaitForSecondsRealtime(flashErrorDuration);
        }

        errorText.color = Color.red;
        errorText.gameObject.SetActive(false);
    }
}
