using UnityEngine;
using Singletons;
using System.Collections; // Required for Coroutines
using TMPro;
using Unity.Cinemachine; 

public enum GameState { Unstarted, Playing, Paused, GameOver, CountingDown }

public class StateMaster : Singleton<StateMaster>
{
    [Header("UI Panels")]
    [SerializeField] private MenuUIManager MenuPanel;
    [SerializeField] private HUDUIManager gameplayHUD;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI countdownText; // Add a text reference

    [SerializeField] private TMP_InputField nameInputField; // Reference the input field
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("References")]
    [SerializeField] private CinemachineCamera mainmenucamera;
    [SerializeField] private CinemachineBrain maincamera;




    [Header("Settings")]
    [SerializeField] private int startCountdownValue = 3;

    public GameState CurrentState { get; private set; }
    public string PlayerName { get; private set; }

    private void Start()
    {
        SetState(GameState.Unstarted);
        gameplayHUD.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        // VALIDATION: Check if name is empty before allowing game start
        if (nameInputField != null && string.IsNullOrWhiteSpace(nameInputField.text))
        {
            StopCoroutine(nameof(FlashErrorRoutine));
            StartCoroutine(FlashErrorRoutine());
            return;
        }

        nameInputField.gameObject.SetActive(false); // Hide input field after starting
        PlayerName = nameInputField.text.Trim(); // Store player name
        mainmenucamera.gameObject.SetActive(false); // Disable main menu camera


        SetState(GameState.Playing);
    }


    public void SetState(GameState newState)
    {
        // Check if we are coming from Unstarted or Paused and moving to Playing
        if (newState == GameState.Playing && (CurrentState == GameState.Unstarted || CurrentState == GameState.Paused))
        {
            StartCoroutine(StartCountdownRoutine());
            return; // Exit here; the Coroutine will call SetState(Playing) at the end
        }

        CurrentState = newState;

        // 1. Handle Time and Logic (Freeze time even during countdown/pause)
        Time.timeScale = (newState == GameState.Playing) ? 1f : 0f;

        // 2. Handle Cursor
        Cursor.visible = (newState != GameState.Playing);
        Cursor.lockState = (newState == GameState.Playing) ? CursorLockMode.Locked : CursorLockMode.None;

        // 3. Handle UI Visibility
        MenuPanel.gameObject.SetActive(newState == GameState.Unstarted || newState == GameState.Paused);
        gameplayHUD.gameObject.SetActive(newState == GameState.Playing || newState == GameState.CountingDown);
        gameOverPanel.SetActive(newState == GameState.GameOver);

        if (newState == GameState.Unstarted || newState == GameState.Paused)
            MenuPanel.Setup();

        Debug.Log($"Game State changed to: {newState}");
    }

    private IEnumerator StartCountdownRoutine()
    {
        CurrentState = GameState.CountingDown;
        CinemachineBlendDefinition originalBlend = maincamera.DefaultBlend; // Store original blend
        
        maincamera.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 2.0f);

        // Setup UI for countdown
        MenuPanel.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);

        float timer = startCountdownValue;

        while (timer > 0)
        {
            countdownText.text = Mathf.CeilToInt(timer).ToString();

            // Use WaitForSecondsRealtime because Time.timeScale is 0 while paused
            yield return new WaitForSecondsRealtime(1f);
            timer--;
        }

        countdownText.gameObject.SetActive(false);
        maincamera.DefaultBlend = originalBlend;


        // Finally, trigger the actual Playing state
        SetState(GameState.Playing);
    }

    private IEnumerator FlashErrorRoutine()
    {
        if (errorText == null) yield break;

        errorText.gameObject.SetActive(true);
        errorText.text = "ENTER NAME!";

        // Simple flash effect (3 times)
        for (int i = 0; i < 3; i++)
        {
            errorText.color = Color.red;
            yield return new WaitForSecondsRealtime(0.1f);
            errorText.color = Color.white;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        errorText.color = Color.red; // Keep it red after flashing
    }

    public void Resume() => SetState(GameState.Playing);
    public void Quit() => Application.Quit();
}
