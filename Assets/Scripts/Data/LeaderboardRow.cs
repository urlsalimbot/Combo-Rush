using UnityEngine;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameText;
    [SerializeField] private TMP_InputField scoreText;
    [SerializeField] private TMP_InputField dateText;

    private void Awake()
    {
        if (nameText == null) Debug.LogError("[LeaderboardRow] nameText not assigned!");
        if (scoreText == null) Debug.LogError("[LeaderboardRow] scoreText not assigned!");
        if (dateText == null) Debug.LogError("[LeaderboardRow] dateText not assigned!");
    }

    public void Setup(string name, float score, string datePlayed)
    {
        Debug.Log($"[LeaderboardRow] Setup called: {name} - {score} - {datePlayed}");
        
        if (nameText != null)
        {
            nameText.text = name;
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        if (dateText != null)
        {
            dateText.text = datePlayed;
        }
    }
}
