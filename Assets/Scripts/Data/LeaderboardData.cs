using System;
using System.Collections.Generic;

[Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public DateTime date;
    public ScoreEntry(string name, int s)
    {
        playerName = name;
        score = s;
        date = DateTime.Now;
    }
}