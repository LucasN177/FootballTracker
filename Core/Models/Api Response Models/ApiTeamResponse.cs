namespace FootballTracker.Core.Models.Api_Response_Models;

public class ApiTeamResponse
{
    public int teamInfoId { get; set; }
    public string teamName { get; set; } = string.Empty;
    public string shortName { get; set; } = string.Empty;
    public string teamIconUrl { get; set; } = string.Empty;
    public int points { get; set; }
    public int opponentGoals { get; set; }
    public int goals { get; set; }
    public int matches { get; set; }
    public int won { get; set; }
    public int lost { get; set; }
    public int draw { get; set; }
    public int goalDiff { get; set; }    
}