using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models;

public class Group
{
    [JsonPropertyName("groupName")]
    public required string GroupName { get; set; }
    
    [JsonPropertyName("groupOrderId")]
    public required int GroupOrderId { get; set; }
    
    [JsonPropertyName("groupId")]
    public required int GroupId { get; set; }
}