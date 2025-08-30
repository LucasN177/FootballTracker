using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models;

public class Game
{
    [JsonPropertyName("matchID")]
        public int MatchId { get; set; }

        [JsonPropertyName("matchDateTime")]
        public DateTime MatchDateTime { get; set; }

        [JsonPropertyName("timeZoneID")]
        public string TimeZoneId { get; set; }

        [JsonPropertyName("leagueId")]
        public int LeagueId { get; set; }

        [JsonPropertyName("leagueName")]
        public string LeagueName { get; set; }

        [JsonPropertyName("leagueSeason")]
        public int LeagueSeason { get; set; }

        [JsonPropertyName("leagueShortcut")]
        public string LeagueShortcut { get; set; }

        [JsonPropertyName("matchDateTimeUTC")]
        public DateTime MatchDateTimeUtc { get; set; }

        [JsonPropertyName("team1")]
        public Team Team1 { get; set; }

        [JsonPropertyName("team2")]
        public Team Team2 { get; set; }

        [JsonPropertyName("lastUpdateDateTime")]
        public DateTime LastUpdateDateTime { get; set; }

        [JsonPropertyName("matchIsFinished")]
        public bool MatchIsFinished { get; set; }

        [JsonPropertyName("matchResults")]
        public List<MatchResult> MatchResults { get; set; } = new List<MatchResult>();

        [JsonPropertyName("goals")]
        public List<Goal> Goals { get; set; } = new List<Goal>();

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("numberOfViewers")]
        public int? NumberOfViewers { get; set; }

        // Computed Properties für einfacheren Zugriff
        public MatchResult FinalResult => MatchResults?.FirstOrDefault(r => r.ResultTypeId == 2);
        public MatchResult HalfTimeResult => MatchResults?.FirstOrDefault(r => r.ResultTypeId == 1);
        public string DisplayResult => FinalResult != null ? $"{FinalResult.PointsTeam1}:{FinalResult.PointsTeam2}" : "-:-";
        public bool IsToday => MatchDateTime.Date == DateTime.Today;
        public bool IsUpcoming => !MatchIsFinished && MatchDateTime > DateTime.Now;
}