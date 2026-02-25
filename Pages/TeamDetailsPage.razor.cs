using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FootballTracker.Core.Models;
using FootballTracker.Services;
using Microsoft.AspNetCore.Components;

namespace FootballTracker.Pages;

public partial class TeamDetailsPage : ComponentBase
{
    [Parameter]
    public string? TeamId { get; set; }

    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private TabellenService TabellenService { get; set; } = null!;
    [Inject] private TeamService TeamService { get; set; } = null!;
    
    private Team Team { get; set; } = new ();
    private List<Game> Matches { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var teams = await TabellenService.GetBundesligaTabelle(2025);
        var team = teams.FirstOrDefault(x => x.Id.ToString() == TeamId);
        if(team != null)
            Team = team;
        Matches = await TeamService.GetGames(Team.Id, 5, 1);
    }
    
    private List<Player> Players = new()
    {
        
    };

    
}