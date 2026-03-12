using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField scoreInputField;
    [SerializeField] private TMP_InputField combosInputField;
    [SerializeField] private TMP_InputField multiplierInputField;



    public void Setup(int finalScore, int totalCombos, float finalMultiplier)
    {
        scoreInputField.text = finalScore.ToString();
        combosInputField.text = totalCombos.ToString();
        multiplierInputField.text = finalMultiplier.ToString("0.00");
    }

    public void OnSubmitScoreClicked(Button submitButton)
    {
        GameMaster.Instance.SubmitScore(int.Parse(scoreInputField.text), int.Parse(combosInputField.text), float.Parse(multiplierInputField.text));
        submitButton.interactable = false; // Disable the button to prevent multiple submissions
        submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submitted!";
        Debug.Log($"Submitting Score: {scoreInputField.text} for Player: {StateMaster.Instance.PlayerName}");
    }
}
