using UnityEngine;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField rankText;
    [SerializeField] private TMP_InputField nameText;
    [SerializeField] private TMP_InputField scoreText;
    [SerializeField] private TMP_InputField dateText;

    private void Awake()
    {
        if (nameText == null) Debug.LogError("[LeaderboardRow] nameText not assigned!");
        if (scoreText == null) Debug.LogError("[LeaderboardRow] scoreText not assigned!");
        if (dateText == null) Debug.LogError("[LeaderboardRow] dateText not assigned!");
    }

    public void Setup(int rank, string name, float score, string datePlayed)
    {
        Debug.Log($"[LeaderboardRow] Setup called: {rank} - {name} - {score} - {datePlayed}");


        rankText.text = $"{rank}.";
        nameText.text = name;

        
        scoreText.text = score.ToString("F2");
        dateText.text = datePlayed;

    }
}
