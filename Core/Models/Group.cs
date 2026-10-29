using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models;

public class Group
{
    [JsonPropertyName("groupName")]
    public string GroupName { get; set; } = string.Empty;
    
    [JsonPropertyName("groupOrderID")]
    public int GroupOrderId { get; set; }
    
    [JsonPropertyName("groupID")]
    public int GroupId { get; set; }
}
