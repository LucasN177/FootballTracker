using System.Text.Json.Serialization;

namespace FootballTracker.Core.Models;

public class Team
{
    [JsonPropertyName("teamInfoId")]
    public int Id { get; set; }

    /// <summary>In Spieldaten liefert OpenLigaDB die Team-ID als "teamId".</summary>
    [JsonPropertyName("teamId")]
    public int TeamId
    {
        get => Id;
        set => Id = value;
    }

    public int TabellenPlatz { get; set; }
    [JsonPropertyName("teamName")]
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    [JsonPropertyName("teamIconUrl")]
    public string LogoUrl { get; set; } = string.Empty;
    public int Spiele { get; set; }
    public int Siege { get; set; }
    public int Unentschieden { get; set; }
    public int Niederlagen { get; set; }
    public int Tore { get; set; }
    public int Gegentore { get; set; }
    public int Differenz => Tore - Gegentore;
    public int Punkte { get; set; }
    /// <summary>Letzte Ergebnisse, älteste zuerst ('S' = Sieg, 'U' = Unentschieden, 'N' = Niederlage).</summary>
    public string Form { get; set; } = string.Empty;
    public Game? NextGame { get; set; }
    public List<Game> LastGames { get; set; } = new();

    public string DisplayName => string.IsNullOrWhiteSpace(ShortName) ? Name : ShortName;
}
