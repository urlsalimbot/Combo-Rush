using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI combosText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private Button submitButton;

    private void Awake()
    {
        // Ensure StateMaster exists
        var stateMaster = StateMaster.Instance;
        if (stateMaster == null)
        {
            Debug.LogError("[GameOverManager] StateMaster not found in scene!");
        }
    }

    private void OnEnable()
    {
        StateMaster.Instance.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        if (StateMaster.Instance != null)
        {
            StateMaster.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleGameOver()
    {
        gameObject.SetActive(true);
    }

    public void Setup(int finalScore, int totalCombos, float finalMultiplier)
    {
        scoreText.text = finalScore.ToString();
        combosText.text = totalCombos.ToString();
        multiplierText.text = finalMultiplier.ToString("F2");
    }

    public void OnSubmitScoreClicked()
    {
        if (submitButton != null)
        {
            GameMaster.Instance.SubmitScore();
            submitButton.interactable = false;
            TextMeshProUGUI buttonText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Submitted!";
            }
        }

        Debug.Log($"Submitting Score: {scoreText.text} for Player: {StateMaster.Instance.PlayerName}");
    }
}
