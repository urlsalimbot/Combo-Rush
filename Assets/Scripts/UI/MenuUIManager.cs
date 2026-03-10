using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;


    public void Setup()
    {
        if (StateMaster.Instance.CurrentState == GameState.Unstarted)
        {
            startButton.gameObject.SetActive(true);
            resumeButton.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(false);
            mainMenuButton.gameObject.SetActive(false);
        }
        else if (StateMaster.Instance.CurrentState == GameState.Playing)
        {
            startButton.gameObject.SetActive(false);
            resumeButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
        }
    }

    public void OnStartClicked()
    {
        StateMaster.Instance.StartGame();
    }

    public void OnResumeClicked()
    {
        // Assuming your PauseManager has a Resume or Toggle function
        StateMaster.Instance.Resume();
    }

    public void OnRestartClicked()
    {
        // Reset time scale just in case your manager doesn't do it on scene load
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void OnLeaderboardClicked()
    {
        // Reset time scale just in case your manager doesn't do it on scene load
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuClicked(string mainMenuSceneName)
    {
        Time.timeScale = 1f;
        // SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // Call this from your PauseManager when the Escape key is hit
    public void ToggleUI(bool isPaused)
    {
        gameObject.SetActive(isPaused);
    }
}
