namespace FootballTracker.Core.Models.Statistics;

public class PlayerSeasonStats
{
    public required int PlayerId { get; init; }
    public required string Name { get; set; }
    public Team? Team { get; set; }
    public int Rank { get; set; }
    public List<GoalEvent> Events { get; } = new();

    public IEnumerable<GoalEvent> Goals => Events.Where(e => !e.Goal.IsOwnGoal);
    public int GoalCount => Goals.Count();
    public int PenaltyCount => Goals.Count(e => e.Goal.IsPenalty);
    public int OwnGoalCount => Events.Count(e => e.Goal.IsOwnGoal);
    public int GamesWithGoal => Goals.Select(e => e.Game.MatchId).Distinct().Count();
    public int Doubles => Goals.GroupBy(e => e.Game.MatchId).Count(g => g.Count() >= 2);
    public GoalEvent? LastGoal => Goals.OrderBy(e => e.Game.MatchDateTimeUtc).ThenBy(e => e.Goal.MatchMinute).LastOrDefault();

    public double? AverageMinute
    {
        get
        {
            var minutes = Goals.Where(e => e.Goal.MatchMinute.HasValue).Select(e => (double)e.Goal.MatchMinute!.Value).ToList();
            return minutes.Count == 0 ? null : minutes.Average();
        }
    }

    public double[] GoalsPerMatchDay(int matchDays)
    {
        var data = new double[matchDays];
        foreach (var goal in Goals.Where(e => e.MatchDay >= 1 && e.MatchDay <= matchDays))
            data[goal.MatchDay - 1]++;
        return data;
    }

    public double[] CumulativeGoals(int matchDays)
    {
        var perDay = GoalsPerMatchDay(matchDays);
        var sum = 0d;
        return perDay.Select(d => sum += d).ToArray();
    }
}
