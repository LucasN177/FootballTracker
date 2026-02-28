namespace FootballTracker.Core.Models;

public class GoalGetter
{
    public int Platzierung { get; set; }
    public required int Id { get; set; }
    
    public required string Name { get; set; }
    
    public required int GoalCount { get; set; }

    public bool IsFavorite { get; set; }
}