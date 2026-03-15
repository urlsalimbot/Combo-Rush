using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;

public class Leaderboard : MonoBehaviour
{
    private const int MaxEntries = 10;
    private const string EncryptionKey = "ComboRush1LETSGO"; // Must be 16, 24, or 32 chars for AES

    [Header("UI References")]
    [SerializeField] private Transform container;
    [SerializeField] private GameObject rowPrefab;

    private string _savePath;
    private LeaderboardData _data = new LeaderboardData();

    private void Awake()
    {
        _savePath = Application.persistentDataPath + "/leaderboard.dat";
        Debug.Log($"[Leaderboard] Awake - Save path: {_savePath}");
        
        if (container == null) Debug.LogWarning("[Leaderboard] Container not assigned!");
        if (rowPrefab == null) Debug.LogWarning("[Leaderboard] Row prefab not assigned!");
        
        LoadLeaderboard();
        Debug.Log($"[Leaderboard] Loaded {_data.Entries.Count} entries");
    }

    public void AddEntry(string name, int score)
    {
        Debug.Log($"[Leaderboard] Adding entry: {name} - {score}");
        
        _data.Entries.Add(new ScoreEntry(name, score));

        // Sort high to low and keep top entries
        _data.Entries = _data.Entries
            .OrderByDescending(s => s.Score)
            .Take(MaxEntries)
            .ToList();

        SaveLeaderboard();
        Debug.Log($"[Leaderboard] Saved { _data.Entries.Count} entries");
    }

    private void SaveLeaderboard()
    {
        try
        {
            string json = JsonUtility.ToJson(_data);
            byte[] encrypted = Encrypt(json);
            File.WriteAllBytes(_savePath, encrypted);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save leaderboard: {e.Message}");
        }
    }

    private void LoadLeaderboard()
    {
        if (!File.Exists(_savePath)) return;

        try
        {
            byte[] encrypted = File.ReadAllBytes(_savePath);
            string json = Decrypt(encrypted);
            _data = JsonUtility.FromJson<LeaderboardData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load leaderboard: {e.Message}");
            _data = new LeaderboardData();
        }
    }

    // --- AES Encryption Logic ---
    private byte[] Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aes.IV = new byte[16];
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return ms.ToArray();
                }
            }
        }
    }

    private string Decrypt(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aes.IV = new byte[16];
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }

    public void RefreshDisplay()
    {
        Debug.Log($"[Leaderboard] RefreshDisplay called - Entries: {_data.Entries.Count}");
        
        if (container == null)
        {
            Debug.LogError("[Leaderboard] Container not assigned! Cannot refresh display.");
            return;
        }
        
        if (rowPrefab == null)
        {
            Debug.LogError("[Leaderboard] Row prefab not assigned! Cannot refresh display.");
            return;
        }

        // Clear old rows
        Debug.Log($"[Leaderboard] Clearing {container.childCount} existing rows");
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        if (_data.Entries.Count == 0)
        {
            Debug.Log("[Leaderboard] No entries to display");
            return;
        }

        // Create rows for each entry
        for (int i = 0; i < _data.Entries.Count; i++)
        {
            var entry = _data.Entries[i];
            Debug.Log($"[Leaderboard] Creating row {i+1}/{_data.Entries.Count} for {entry.PlayerName}");
            
            GameObject go = Instantiate(rowPrefab, container);
            
            LeaderboardRow row = go.GetComponent<LeaderboardRow>();
            if (row == null)
            {
                Debug.LogError($"[Leaderboard] Instantiated object does not have LeaderboardRow component!");
                continue;
            }
            
            row.Setup(entry.PlayerName, entry.Score, entry.Date);
        }
        
        Debug.Log($"[Leaderboard] Displayed {_data.Entries.Count} entries");
    }
}
