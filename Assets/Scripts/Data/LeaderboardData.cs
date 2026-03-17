using System;
using System.Collections.Generic;

[Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> Entries = new List<ScoreEntry>();
}

[Serializable]
public class ScoreEntry
{
    public string PlayerName;
    public float Score;
    public string Date;

    public ScoreEntry(string name, float score)
    {
        PlayerName = name;
        Score = score;
        Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // Parameterless constructor required for JsonUtility
    public ScoreEntry()
    {
        PlayerName = string.Empty;
        Score = 0;
        Date = string.Empty;
    }
}
