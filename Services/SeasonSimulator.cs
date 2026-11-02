using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Statistics;

namespace FootballTracker.Services;

/// <summary>
/// Spielt die offenen Spiele einer Saison wiederholt zufällig nach den Ergebniswahrscheinlichkeiten des Modells durch
/// und zählt, wie oft jedes Team auf welchem Platz landet. Die Teamstärken bleiben während der Simulation fest.
/// Reihenfolge bei Gleichstand: Punkte, Tordifferenz, erzielte Tore, danach Los.
/// </summary>
public sealed class SeasonSimulator
{
    private readonly record struct OpenGame(int Home, int Away, double[] Cumulative, int Columns);

    private readonly Team[] _teams;
    private readonly int[] _points;
    private readonly int[] _goalDifference;
    private readonly int[] _goalsFor;
    private readonly int[] _played;
    private readonly OpenGame[] _openGames;
    private readonly Random _random;

    private readonly long[,] _positionCounts;
    private readonly long[] _pointsSum;

    public int Runs { get; private set; }
    public int RemainingGames => _openGames.Length;

    /// <param name="games">Alle Spiele, die in die Tabelle eingehen (beendete und offene).</param>
    /// <param name="predict">Liefert die Ergebniswahrscheinlichkeiten eines offenen Spiels.</param>
    public SeasonSimulator(IReadOnlyCollection<Game> games, Func<Game, MatchPrediction> predict, int seed)
    {
        _teams = games.SelectMany(g => new[] { g.Team1, g.Team2 }).DistinctBy(t => t.Id).ToArray();
        var index = _teams.Select((t, i) => (t.Id, i)).ToDictionary(x => x.Id, x => x.i);
        var n = _teams.Length;
        _points = new int[n];
        _goalDifference = new int[n];
        _goalsFor = new int[n];
        _played = new int[n];

        var open = new List<OpenGame>();
        foreach (var game in games)
        {
            int home = index[game.Team1.Id], away = index[game.Team2.Id];
            if (game.MatchIsFinished && game.FinalResult is { } result)
            {
                AddResult(_points, _goalDifference, _goalsFor, home, away, result.PointsTeam1, result.PointsTeam2);
                _played[home]++;
                _played[away]++;
            }
            else
            {
                var probabilities = predict(game).ScoreProbabilities;
                var columns = probabilities.GetLength(1);
                var cumulative = new double[probabilities.Length];
                double sum = 0;
                for (var i = 0; i < probabilities.GetLength(0); i++)
                    for (var j = 0; j < columns; j++)
                        cumulative[i * columns + j] = sum += probabilities[i, j];
                open.Add(new OpenGame(home, away, cumulative, columns));
            }
        }

        _openGames = open.ToArray();
        _random = new Random(seed);
        _positionCounts = new long[n, n];
        _pointsSum = new long[n];
    }

    public void Run(int runs)
    {
        var n = _teams.Length;
        var points = new int[n];
        var goalDifference = new int[n];
        var goalsFor = new int[n];
        var lots = new double[n];
        var order = new int[n];

        for (var run = 0; run < runs; run++)
        {
            Array.Copy(_points, points, n);
            Array.Copy(_goalDifference, goalDifference, n);
            Array.Copy(_goalsFor, goalsFor, n);

            foreach (var game in _openGames)
            {
                var cell = Sample(game.Cumulative, _random.NextDouble());
                AddResult(points, goalDifference, goalsFor, game.Home, game.Away, cell / game.Columns, cell % game.Columns);
            }

            for (var i = 0; i < n; i++)
            {
                order[i] = i;
                lots[i] = _random.NextDouble();
            }
            Array.Sort(order, (a, b) =>
                points[a] != points[b] ? points[b].CompareTo(points[a])
                : goalDifference[a] != goalDifference[b] ? goalDifference[b].CompareTo(goalDifference[a])
                : goalsFor[a] != goalsFor[b] ? goalsFor[b].CompareTo(goalsFor[a])
                : lots[a].CompareTo(lots[b]));

            for (var position = 0; position < n; position++)
            {
                _positionCounts[order[position], position]++;
                _pointsSum[order[position]] += points[order[position]];
            }
        }
        Runs += runs;
    }

    public SeasonSimulation GetResult()
    {
        var n = _teams.Length;
        var teams = Enumerable.Range(0, n).Select(i =>
        {
            var probabilities = Enumerable.Range(0, n).Select(p => Runs == 0 ? 0 : (double)_positionCounts[i, p] / Runs).ToArray();
            return new SimulatedTeam
            {
                Team = _teams[i],
                Points = _points[i],
                GoalDifference = _goalDifference[i],
                Played = _played[i],
                ExpectedPoints = Runs == 0 ? _points[i] : (double)_pointsSum[i] / Runs,
                ExpectedPosition = probabilities.Select((p, position) => p * (position + 1)).Sum(),
                PositionProbabilities = probabilities
            };
        });

        return new SeasonSimulation
        {
            Runs = Runs,
            RemainingGames = _openGames.Length,
            Teams = teams.OrderBy(t => t.ExpectedPosition).ThenByDescending(t => t.ExpectedPoints).ToList()
        };
    }

    /// <summary>Index der ersten kumulierten Wahrscheinlichkeit, die größer als <paramref name="value"/> ist.</summary>
    private static int Sample(double[] cumulative, double value)
    {
        value *= cumulative[^1];
        int low = 0, high = cumulative.Length - 1;
        while (low < high)
        {
            var mid = (low + high) / 2;
            if (cumulative[mid] > value)
                high = mid;
            else
                low = mid + 1;
        }
        return low;
    }

    private static void AddResult(int[] points, int[] goalDifference, int[] goalsFor, int home, int away, int homeGoals, int awayGoals)
    {
        goalsFor[home] += homeGoals;
        goalsFor[away] += awayGoals;
        goalDifference[home] += homeGoals - awayGoals;
        goalDifference[away] += awayGoals - homeGoals;
        if (homeGoals > awayGoals)
            points[home] += 3;
        else if (homeGoals < awayGoals)
            points[away] += 3;
        else
        {
            points[home]++;
            points[away]++;
        }
    }
}
