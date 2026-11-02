using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Statistics;

namespace FootballTracker.Services;

/// <summary>
/// Dixon-Coles-Modell: Die Tore beider Teams sind Poisson-verteilt mit
/// λ_Heim = exp(μ + Heimvorteil + Angriff_Heim + Abwehr_Gast) und λ_Gast = exp(μ + Angriff_Gast + Abwehr_Heim).
/// Die Parameter werden per gewichteter Maximum-Likelihood geschätzt, dadurch fließt die Stärke der bisherigen Gegner ein.
/// ρ korrigiert die Wahrscheinlichkeiten der knappen Ergebnisse 0:0, 1:0, 0:1 und 1:1.
/// </summary>
public sealed class PredictionModel
{
    /// <summary>Ein Spiel als Trainingsdatum. Das Gewicht sinkt mit dem Alter des Spiels.</summary>
    public readonly record struct Sample(int HomeId, int AwayId, int HomeGoals, int AwayGoals, double Weight);

    /// <summary>Stärke eines Teams auf logarithmischer Skala (0 = Ligaschnitt, Abwehr: negativ = weniger Gegentore).</summary>
    public readonly record struct Rating(double Attack, double Defense);

    /// <summary>Gewicht eines Spiels halbiert sich nach dieser Zeit.</summary>
    public const double HalfLifeDays = 270;

    /// <summary>
    /// Zieht die Teamwerte zum Ligaschnitt bzw. zum Aufsteiger-Startwert (wirkt wie etwa acht zusätzliche Durchschnittsspiele).
    /// Werte für Halbwertszeit und Regularisierung stammen aus einem Backtest über 1. und 2. Bundesliga 2021/22 bis 2025/26.
    /// </summary>
    private const double Regularization = 12.0;

    /// <summary>Startwerte für Teams, die in der Vorsaison nicht in der Liga waren: etwa 20 % weniger Tore, 20 % mehr Gegentore.</summary>
    private const double NewTeamAttackPrior = -0.2;
    private const double NewTeamDefensePrior = 0.2;

    private const int MaxIterations = 300;
    private const double Tolerance = 1e-3;
    public const int MaxGoals = 10;

    private readonly Dictionary<int, Rating> _ratings;
    private readonly HashSet<int> _newTeams;

    public double Intercept { get; }
    public double HomeAdvantage { get; }
    public double Rho { get; }
    public int TrainingGames { get; }
    public IReadOnlyDictionary<int, Rating> Ratings => _ratings;

    /// <summary>Erwartete Tore eines durchschnittlichen Teams ohne Heimvorteil (also auswärts).</summary>
    public double BaseGoals => Math.Exp(Intercept);

    /// <summary>Heimvorteil als Faktor auf die erwarteten Tore, z.B. 1,2 = 20 % mehr.</summary>
    public double HomeFactor => Math.Exp(HomeAdvantage);

    private PredictionModel(double intercept, double homeAdvantage, double rho, Dictionary<int, Rating> ratings, HashSet<int> newTeams, int trainingGames)
    {
        Intercept = intercept;
        HomeAdvantage = homeAdvantage;
        Rho = rho;
        _ratings = ratings;
        _newTeams = newTeams;
        TrainingGames = trainingGames;
    }

    public static double GetWeight(DateTime kickoffUtc, DateTime referenceUtc) =>
        Math.Pow(0.5, Math.Max(0, (referenceUtc - kickoffUtc).TotalDays) / HalfLifeDays);

    /// <param name="samples">Beendete Spiele mit Gewicht.</param>
    /// <param name="teamIds">Alle Teams, für die Prognosen gebraucht werden – auch ohne bisherige Spiele.</param>
    /// <param name="newTeams">Teams ohne Vorsaison in der Liga (z.B. Aufsteiger).</param>
    public static PredictionModel Fit(IReadOnlyList<Sample> samples, IEnumerable<int> teamIds, IReadOnlySet<int> newTeams)
    {
        var ids = teamIds.Concat(samples.SelectMany(s => new[] { s.HomeId, s.AwayId })).Distinct().ToList();
        var index = ids.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        var n = ids.Count;

        var attackPrior = ids.Select(id => newTeams.Contains(id) ? NewTeamAttackPrior : 0).ToArray();
        var defensePrior = ids.Select(id => newTeams.Contains(id) ? NewTeamDefensePrior : 0).ToArray();
        var attack = (double[])attackPrior.Clone();
        var defense = (double[])defensePrior.Clone();

        var games = samples.Select(s => (Home: index[s.HomeId], Away: index[s.AwayId], s.HomeGoals, s.AwayGoals, s.Weight)).ToArray();
        var totalWeight = games.Sum(g => g.Weight);
        var averageGoals = totalWeight > 0 ? games.Sum(g => g.Weight * (g.HomeGoals + g.AwayGoals)) / (2 * totalWeight) : 1.4;
        var intercept = Math.Log(Math.Max(averageGoals, 0.1));
        var home = totalWeight > 0 ? 0.2 : 0.25;

        var gradient = new double[n];
        var hessian = new double[n];

        // Koordinatenweises Newton-Verfahren: Jede Parametergruppe hängt bei festen übrigen Parametern
        // nur von ihren eigenen Spielen ab, daher lassen sich alle Angriffs- bzw. Abwehrwerte gleichzeitig anpassen.
        for (var iteration = 0; iteration < MaxIterations && totalWeight > 0; iteration++)
        {
            double maxStep = 0;

            // Grundniveau μ
            double gradient1 = 0, hessian1 = 0;
            foreach (var g in games)
            {
                var lambdaHome = Math.Exp(intercept + home + attack[g.Home] + defense[g.Away]);
                var lambdaAway = Math.Exp(intercept + attack[g.Away] + defense[g.Home]);
                gradient1 += g.Weight * (g.HomeGoals - lambdaHome + g.AwayGoals - lambdaAway);
                hessian1 += g.Weight * (lambdaHome + lambdaAway);
            }
            var stepMu = Clamp(gradient1 / hessian1);
            intercept += stepMu;

            // Heimvorteil
            gradient1 = hessian1 = 0;
            foreach (var g in games)
            {
                var lambdaHome = Math.Exp(intercept + home + attack[g.Home] + defense[g.Away]);
                gradient1 += g.Weight * (g.HomeGoals - lambdaHome);
                hessian1 += g.Weight * lambdaHome;
            }
            var stepHome = Clamp(gradient1 / hessian1);
            home += stepHome;
            maxStep = Math.Max(maxStep, Math.Max(Math.Abs(stepMu), Math.Abs(stepHome)));

            // Angriff
            Array.Clear(gradient);
            Array.Clear(hessian);
            foreach (var g in games)
            {
                var lambdaHome = Math.Exp(intercept + home + attack[g.Home] + defense[g.Away]);
                var lambdaAway = Math.Exp(intercept + attack[g.Away] + defense[g.Home]);
                gradient[g.Home] += g.Weight * (g.HomeGoals - lambdaHome);
                hessian[g.Home] += g.Weight * lambdaHome;
                gradient[g.Away] += g.Weight * (g.AwayGoals - lambdaAway);
                hessian[g.Away] += g.Weight * lambdaAway;
            }
            for (var i = 0; i < n; i++)
            {
                var step = Clamp((gradient[i] - Regularization * (attack[i] - attackPrior[i])) / (hessian[i] + Regularization));
                attack[i] += step;
                maxStep = Math.Max(maxStep, Math.Abs(step));
            }

            // Abwehr
            Array.Clear(gradient);
            Array.Clear(hessian);
            foreach (var g in games)
            {
                var lambdaHome = Math.Exp(intercept + home + attack[g.Home] + defense[g.Away]);
                var lambdaAway = Math.Exp(intercept + attack[g.Away] + defense[g.Home]);
                gradient[g.Away] += g.Weight * (g.HomeGoals - lambdaHome);
                hessian[g.Away] += g.Weight * lambdaHome;
                gradient[g.Home] += g.Weight * (g.AwayGoals - lambdaAway);
                hessian[g.Home] += g.Weight * lambdaAway;
            }
            for (var i = 0; i < n; i++)
            {
                var step = Clamp((gradient[i] - Regularization * (defense[i] - defensePrior[i])) / (hessian[i] + Regularization));
                defense[i] += step;
                maxStep = Math.Max(maxStep, Math.Abs(step));
            }

            if (maxStep < Tolerance)
                break;
        }

        var rho = FitRho(games.Select(g => (
            g.HomeGoals, g.AwayGoals, g.Weight,
            LambdaHome: Math.Exp(intercept + home + attack[g.Home] + defense[g.Away]),
            LambdaAway: Math.Exp(intercept + attack[g.Away] + defense[g.Home]))).ToList());

        var ratings = ids.ToDictionary(id => id, id => new Rating(attack[index[id]], defense[index[id]]));
        return new PredictionModel(intercept, home, rho, ratings, newTeams.ToHashSet(), samples.Count);
    }

    public MatchPrediction Predict(Game game)
    {
        var homeRating = GetRating(game.Team1.Id);
        var awayRating = GetRating(game.Team2.Id);
        var lambdaHome = Math.Exp(Intercept + HomeAdvantage + homeRating.Attack + awayRating.Defense);
        var lambdaAway = Math.Exp(Intercept + awayRating.Attack + homeRating.Defense);

        var homePmf = PoissonPmf(lambdaHome);
        var awayPmf = PoissonPmf(lambdaAway);
        var matrix = new double[MaxGoals + 1, MaxGoals + 1];
        double total = 0;
        for (var i = 0; i <= MaxGoals; i++)
            for (var j = 0; j <= MaxGoals; j++)
            {
                matrix[i, j] = homePmf[i] * awayPmf[j] * Tau(i, j, lambdaHome, lambdaAway, Rho);
                total += matrix[i, j];
            }

        double homeWin = 0, draw = 0, awayWin = 0, best = -1;
        var mostLikely = (0, 0);
        for (var i = 0; i <= MaxGoals; i++)
            for (var j = 0; j <= MaxGoals; j++)
            {
                var p = matrix[i, j] /= total;
                if (i > j) homeWin += p;
                else if (i == j) draw += p;
                else awayWin += p;
                if (p > best)
                {
                    best = p;
                    mostLikely = (i, j);
                }
            }

        var (tip, tipPoints) = FindBestTip(matrix);
        return new MatchPrediction
        {
            Game = game,
            ExpectedGoalsHome = lambdaHome,
            ExpectedGoalsAway = lambdaAway,
            ScoreProbabilities = matrix,
            HomeWin = homeWin,
            Draw = draw,
            AwayWin = awayWin,
            MostLikelyScore = mostLikely,
            MostLikelyProbability = best,
            Tip = tip,
            TipExpectedPoints = tipPoints
        };
    }

    private Rating GetRating(int teamId) =>
        _ratings.TryGetValue(teamId, out var rating) ? rating
        : _newTeams.Contains(teamId) ? new Rating(NewTeamAttackPrior, NewTeamDefensePrior)
        : new Rating(0, 0);

    /// <summary>Tipp mit den höchsten erwarteten Kicktipp-Punkten über alle möglichen Ergebnisse.</summary>
    private static ((int Home, int Away) Tip, double Points) FindBestTip(double[,] matrix)
    {
        var bestTip = (0, 0);
        var bestPoints = -1.0;
        for (var tipHome = 0; tipHome <= 5; tipHome++)
            for (var tipAway = 0; tipAway <= 5; tipAway++)
            {
                double points = 0;
                for (var i = 0; i <= MaxGoals; i++)
                    for (var j = 0; j <= MaxGoals; j++)
                        points += matrix[i, j] * MatchPrediction.GetKicktippPoints((tipHome, tipAway), (i, j));
                if (points > bestPoints)
                {
                    bestPoints = points;
                    bestTip = (tipHome, tipAway);
                }
            }
        return (bestTip, bestPoints);
    }

    /// <summary>ρ per Rastersuche: maximiert die gewichtete Log-Likelihood der Dixon-Coles-Korrektur.</summary>
    private static double FitRho(List<(int HomeGoals, int AwayGoals, double Weight, double LambdaHome, double LambdaAway)> games)
    {
        var bestRho = 0.0;
        var bestLikelihood = double.NegativeInfinity;
        for (var rho = -0.25; rho <= 0.15 + 1e-9; rho += 0.01)
        {
            double likelihood = 0;
            foreach (var g in games.Where(g => g.HomeGoals <= 1 && g.AwayGoals <= 1))
            {
                var tau = Tau(g.HomeGoals, g.AwayGoals, g.LambdaHome, g.LambdaAway, rho);
                likelihood += tau > 0 ? g.Weight * Math.Log(tau) : double.NegativeInfinity;
            }
            if (likelihood > bestLikelihood)
            {
                bestLikelihood = likelihood;
                bestRho = Math.Round(rho, 2);
            }
        }
        return bestRho;
    }

    private static double Tau(int homeGoals, int awayGoals, double lambdaHome, double lambdaAway, double rho) => (homeGoals, awayGoals) switch
    {
        (0, 0) => 1 - lambdaHome * lambdaAway * rho,
        (0, 1) => 1 + lambdaHome * rho,
        (1, 0) => 1 + lambdaAway * rho,
        (1, 1) => 1 - rho,
        _ => 1
    };

    private static double[] PoissonPmf(double lambda)
    {
        var pmf = new double[MaxGoals + 1];
        pmf[0] = Math.Exp(-lambda);
        for (var k = 1; k <= MaxGoals; k++)
            pmf[k] = pmf[k - 1] * lambda / k;
        return pmf;
    }

    private static double Clamp(double step) => Math.Clamp(step, -1, 1);
}
