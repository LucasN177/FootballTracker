namespace FootballTracker.Core.Models;

public class Player
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Image { get; set; }
    public int Number { get; set; }
    public string Position { get; set; }
}