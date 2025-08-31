using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models;

public class Goal
{
    [JsonPropertyName("goalID")]
    public int GoalId { get; set; }

    [JsonPropertyName("scoreTeam1")]
    public int ScoreTeam1 { get; set; }

    [JsonPropertyName("scoreTeam2")]
    public int ScoreTeam2 { get; set; }

    [JsonPropertyName("matchMinute")]
    public int? MatchMinute { get; set; }

    [JsonPropertyName("goalGetterID")]
    public int GoalGetterId { get; set; }

    [JsonPropertyName("goalGetterName")]
    public string GoalGetterName { get; set; }

    [JsonPropertyName("isPenalty")]
    public bool IsPenalty { get; set; }

    [JsonPropertyName("isOwnGoal")]
    public bool IsOwnGoal { get; set; }

    [JsonPropertyName("isOvertime")]
    public bool IsOvertime { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }
}
