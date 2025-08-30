namespace FootballTracker.Core.Models;

public class Team
{
    public int Id { get; set; }
    public int TabellenPlatz { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public int Spiele { get; set; }
    public int Siege { get; set; }
    public int Unentschieden { get; set; }
    public int Niederlagen { get; set; }
    public int Tore { get; set; }
    public int Gegentore { get; set; }
    public int Differenz => Tore - Gegentore;
    public int Punkte { get; set; }
    
}