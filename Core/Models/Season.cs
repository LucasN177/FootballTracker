namespace FootballTracker.Core.Models;

public class Season
{
    public required string Name { get; set; }
    public required int Year { get; set; }

    public static Season FromYear(int year) => new() { Name = $"{year}/{year + 1}", Year = year };
}
