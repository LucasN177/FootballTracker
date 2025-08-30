using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models;

public class MatchResult
{
    [JsonPropertyName("resultID")]
    public int ResultId { get; set; }

    [JsonPropertyName("resultName")]
    public string ResultName { get; set; }

    [JsonPropertyName("pointsTeam1")]
    public int PointsTeam1 { get; set; }

    [JsonPropertyName("pointsTeam2")]
    public int PointsTeam2 { get; set; }

    [JsonPropertyName("resultOrderID")]
    public int ResultOrderId { get; set; }

    [JsonPropertyName("resultTypeID")]
    public int ResultTypeId { get; set; }

    [JsonPropertyName("resultDescription")]
    public string ResultDescription { get; set; }
}