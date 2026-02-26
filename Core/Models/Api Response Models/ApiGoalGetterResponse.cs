using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models.Api_Response_Models;

public class ApiGoalGetterResponse
{
    [JsonPropertyName("goalGetterId")]
    public required int GoalGetterId { get; set; }
    
    [JsonPropertyName("goalGetterName")]
    public required string GoalGetterName { get; set; }
    
    [JsonPropertyName("goalCount")]
    public required int GoalCount { get; set; }
}