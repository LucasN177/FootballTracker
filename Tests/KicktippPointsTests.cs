using FootballTracker.Core.Models.Statistics;

namespace FootballTracker.Tests;

public class KicktippPointsTests
{
    [Theory]
    [InlineData(2, 1, 2, 1, 4)] // exakt
    [InlineData(2, 1, 3, 2, 3)] // Tordifferenz
    [InlineData(2, 1, 3, 0, 2)] // nur Tendenz
    [InlineData(2, 1, 1, 1, 0)] // falsche Tendenz
    [InlineData(1, 2, 0, 1, 3)] // Tordifferenz beim Auswärtssieg
    [InlineData(1, 1, 1, 1, 4)] // exaktes Remis
    [InlineData(1, 1, 2, 2, 2)] // Remis: keine Tordifferenz-Punkte
    [InlineData(0, 0, 1, 0, 0)]
    public void Scores_like_kicktipp_default_rules(int tipHome, int tipAway, int home, int away, int expected)
    {
        Assert.Equal(expected, MatchPrediction.GetKicktippPoints((tipHome, tipAway), (home, away)));
    }

    [Theory]
    [InlineData(3, 0, MatchOutcome.HomeWin)]
    [InlineData(2, 2, MatchOutcome.Draw)]
    [InlineData(0, 1, MatchOutcome.AwayWin)]
    public void Outcome_follows_score(int home, int away, MatchOutcome expected)
    {
        Assert.Equal(expected, MatchPrediction.GetOutcome((home, away)));
    }
}
