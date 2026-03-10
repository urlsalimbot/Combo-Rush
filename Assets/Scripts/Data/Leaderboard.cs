using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

public class Leaderboard : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private Transform container;
    [SerializeField] private GameObject rowPrefab;

    private string savePath;
    private string encryptionKey = "ComboRush1LETSGO"; // Must be 16, 24, or 32 chars
    private LeaderboardData data = new LeaderboardData();

    void Awake()
    {
        savePath = Application.persistentDataPath + "/leaderboard.dat";
        LoadLeaderboard();
    }

    public void AddEntry(string name, int score)
    {
        data.entries.Add(new ScoreEntry(name, score));
        // Sort high to low and keep top 10
        data.entries = data.entries.OrderByDescending(s => s.score).Take(10).ToList();
        SaveLeaderboard();
    }

    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(data);
        byte[] encrypted = Encrypt(json);
        File.WriteAllBytes(savePath, encrypted);
    }

    private void LoadLeaderboard()
    {
        if (!File.Exists(savePath)) return;

        byte[] encrypted = File.ReadAllBytes(savePath);
        string json = Decrypt(encrypted);
        data = JsonUtility.FromJson<LeaderboardData>(json);
    }

    // --- AES Encryption Logic ---
    private byte[] Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
            aes.IV = new byte[16]; // Use a fixed IV or store it for more security
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs)) sw.Write(plainText);
                    return ms.ToArray();
                }
            }
        }
    }

    private string Decrypt(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
            aes.IV = new byte[16];
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs)) return sr.ReadToEnd();
                }
            }
        }
    }

    public void RefreshDisplay()
    {
        if (container == null || rowPrefab == null) return;

        // Clear old rows
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Loop through the loaded data
        foreach (var entry in data.entries)
        {
            GameObject go = Instantiate(rowPrefab, container);

            // Pass name and score. 
            // Note: If you don't have entry.date in your ScoreEntry class, remove it here.
            go.GetComponent<LeaderboardRow>().Setup(entry.playerName, entry.score, entry.date);
        }
    }
}
