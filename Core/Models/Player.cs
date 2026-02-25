namespace FootballTracker.Core.Models;

public class Player
{
    public required string Name { get; set; }
    public string Image { get; set; }
    public int Number { get; set; }
    public string Position { get; set; }
}