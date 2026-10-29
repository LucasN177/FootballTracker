using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models;

public class Game
{
        /// <summary>Zeitfenster, in dem ein begonnenes, nicht beendetes Spiel als "live" gilt.</summary>
        public static readonly TimeSpan LiveWindow = TimeSpan.FromMinutes(150);

        [JsonPropertyName("matchID")]
        public int MatchId { get; set; }

        [JsonPropertyName("matchDateTime")]
        public DateTime MatchDateTime { get; set; }

        [JsonPropertyName("timeZoneID")]
        public string? TimeZoneId { get; set; }

        [JsonPropertyName("leagueId")]
        public int LeagueId { get; set; }

        [JsonPropertyName("leagueName")]
        public string? LeagueName { get; set; }

        [JsonPropertyName("leagueSeason")]
        public int LeagueSeason { get; set; }

        [JsonPropertyName("leagueShortcut")]
        public string? LeagueShortcut { get; set; }

        [JsonPropertyName("matchDateTimeUTC")]
        public DateTime MatchDateTimeUtc { get; set; }

        [JsonPropertyName("group")]
        public Group? Group { get; set; }

        [JsonPropertyName("team1")]
        public Team Team1 { get; set; } = new();

        [JsonPropertyName("team2")]
        public Team Team2 { get; set; } = new();

        [JsonPropertyName("lastUpdateDateTime")]
        public DateTime? LastUpdateDateTime { get; set; }

        [JsonPropertyName("matchIsFinished")]
        public bool MatchIsFinished { get; set; }

        [JsonPropertyName("matchResults")]
        public List<MatchResult> MatchResults { get; set; } = new List<MatchResult>();

        [JsonPropertyName("goals")]
        public List<Goal> Goals { get; set; } = new List<Goal>();

        [JsonPropertyName("location")]
        public MatchLocation? Location { get; set; }

        [JsonPropertyName("numberOfViewers")]
        public int? NumberOfViewers { get; set; }

        public int MatchDay => Group?.GroupOrderId ?? 0;
        public MatchResult? FinalResult => MatchResults.FirstOrDefault(r => r.ResultTypeId == 2);
        public MatchResult? HalfTimeResult => MatchResults.FirstOrDefault(r => r.ResultTypeId == 1);
        public string DisplayResult => FinalResult != null ? $"{FinalResult.PointsTeam1}:{FinalResult.PointsTeam2}" : "-:-";
        public bool IsToday => MatchDateTime.Date == DateTime.Today;
        public bool HasStarted => StartUtc <= DateTime.UtcNow;
        public bool IsUpcoming => !MatchIsFinished && !HasStarted;
        public bool IsLive => !MatchIsFinished && HasStarted && DateTime.UtcNow - StartUtc < LiveWindow;

        /// <summary>Aktueller Spielstand: Endergebnis, sonst letztes Tor, sonst Halbzeitstand.</summary>
        public (int Team1, int Team2)? CurrentScore
        {
                get
                {
                        if (FinalResult != null)
                                return (FinalResult.PointsTeam1, FinalResult.PointsTeam2);
                        var lastGoal = Goals.LastOrDefault();
                        if (lastGoal != null)
                                return (lastGoal.ScoreTeam1, lastGoal.ScoreTeam2);
                        if (HalfTimeResult != null)
                                return (HalfTimeResult.PointsTeam1, HalfTimeResult.PointsTeam2);
                        return HasStarted ? (0, 0) : null;
                }
        }

        private DateTime StartUtc => MatchDateTimeUtc == default
                ? MatchDateTime.ToUniversalTime()
                : DateTime.SpecifyKind(MatchDateTimeUtc, DateTimeKind.Utc);
}
