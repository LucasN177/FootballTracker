using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Statistics;

namespace FootballTracker.Tests;

/// <summary>Hilfsmethoden zum Erzeugen von Spielen und Prognosen ohne OpenLigaDB.</summary>
internal static class TestData
{
    private static readonly DateTime SeasonStart = new(2025, 8, 1, 15, 30, 0, DateTimeKind.Utc);

    public static Team Team(int id) => new() { Id = id, Name = $"Team {id}", ShortName = $"T{id}" };

    public static Game Finished(int home, int away, int homeGoals, int awayGoals, int matchDay = 1) => new()
    {
        Team1 = Team(home),
        Team2 = Team(away),
        Group = new Group { GroupOrderId = matchDay },
        MatchDateTimeUtc = SeasonStart.AddDays(7 * (matchDay - 1)),
        MatchIsFinished = true,
        MatchResults = [new MatchResult { ResultTypeId = 2, PointsTeam1 = homeGoals, PointsTeam2 = awayGoals }]
    };

    public static Game Open(int home, int away, int matchDay = 1) => new()
    {
        Team1 = Team(home),
        Team2 = Team(away),
        Group = new Group { GroupOrderId = matchDay },
        MatchDateTimeUtc = SeasonStart.AddDays(7 * (matchDay - 1))
    };

    /// <summary>Prognose, bei der genau ein Ergebnis sicher ist.</summary>
    public static MatchPrediction CertainResult(Game game, int homeGoals, int awayGoals)
    {
        var matrix = new double[11, 11];
        matrix[homeGoals, awayGoals] = 1;
        return Prediction(game, matrix);
    }

    /// <summary>Prognose aus einer Ergebnis-Matrix; Sieg/Remis/Niederlage werden daraus berechnet.</summary>
    public static MatchPrediction Prediction(Game game, double[,] matrix, (int, int)? tip = null)
    {
        double homeWin = 0, draw = 0, awayWin = 0;
        for (var i = 0; i < matrix.GetLength(0); i++)
            for (var j = 0; j < matrix.GetLength(1); j++)
            {
                if (i > j) homeWin += matrix[i, j];
                else if (i == j) draw += matrix[i, j];
                else awayWin += matrix[i, j];
            }

        return new MatchPrediction
        {
            Game = game,
            ScoreProbabilities = matrix,
            HomeWin = homeWin,
            Draw = draw,
            AwayWin = awayWin,
            Tip = tip ?? (homeWin >= awayWin ? (2, 1) : (1, 2)),
            DrawTip = (1, 1)
        };
    }

    /// <summary>
    /// Erzeugt mehrere Doppelrunden einer Liga mit bekannten Stärken. Die Tore sind Poisson-verteilt wie im Modell angenommen.
    /// </summary>
    public static List<Game> SimulateLeague(double[] attack, double[] defense, double homeAdvantage, double baseGoals, int rounds, int seed)
    {
        var random = new Random(seed);
        var games = new List<Game>();
        var n = attack.Length;
        var index = 0;
        for (var round = 0; round < rounds; round++)
            for (var home = 0; home < n; home++)
                for (var away = 0; away < n; away++)
                {
                    if (home == away)
                        continue;
                    var lambdaHome = baseGoals * Math.Exp(homeAdvantage + attack[home] + defense[away]);
                    var lambdaAway = baseGoals * Math.Exp(attack[away] + defense[home]);
                    games.Add(Finished(home + 1, away + 1, Poisson(random, lambdaHome), Poisson(random, lambdaAway), 1 + index++ / (n / 2)));
                }
        return games;
    }

    /// <summary>Knuth-Verfahren: zählt Zufallszahlen, bis ihr Produkt unter e^-λ fällt.</summary>
    private static int Poisson(Random random, double lambda)
    {
        var limit = Math.Exp(-lambda);
        var product = random.NextDouble();
        var k = 0;
        while (product > limit)
        {
            product *= random.NextDouble();
            k++;
        }
        return k;
    }
}
