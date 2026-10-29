using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models;

public class MatchLocation
{
    [JsonPropertyName("locationID")]
    public int LocationId { get; set; }

    [JsonPropertyName("locationCity")]
    public string? City { get; set; }

    [JsonPropertyName("locationStadium")]
    public string? Stadium { get; set; }
}
