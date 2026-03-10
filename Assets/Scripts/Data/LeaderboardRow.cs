using UnityEngine;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField nameText;
    [SerializeField] private TMPro.TMP_InputField scoreText;
    [SerializeField] private TMPro.TMP_InputField date;

    public void Setup(string name, int score, string dateplayed)
    {
        nameText.text = name;
        scoreText.text = score.ToString();
        date.text = dateplayed;
    }
}
