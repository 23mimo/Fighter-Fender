using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tutorial
{
    public class LeaderboardEntry
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }

    public class Leaderboard
    {
        private readonly string filePath = "leaderboard.txt";
        public List<LeaderboardEntry> HighScoresWithNames { get; private set; } = new List<LeaderboardEntry>();

        public void Load()
        {
            if (File.Exists(filePath))
            {
                HighScoresWithNames = File.ReadAllLines(filePath)
                    .Select(line =>
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int score))
                            return new LeaderboardEntry { Name = parts[0], Score = score };
                        return null;
                    })
                    .Where(e => e != null)
                    .OrderByDescending(e => e.Score)
                    .Take(10)
                    .ToList();
            }
        }

        public void AddScoreWithName(int score, string name)
        {
            HighScoresWithNames.Add(new LeaderboardEntry { Name = name, Score = score });
            HighScoresWithNames = HighScoresWithNames.OrderByDescending(e => e.Score).Take(10).ToList();
            File.WriteAllLines(filePath, HighScoresWithNames.Select(e => $"{e.Name}|{e.Score}"));
        }
    }
}
