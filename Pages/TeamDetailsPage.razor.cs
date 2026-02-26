using FootballTracker.Core.Models;
using FootballTracker.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FootballTracker.Pages;

public partial class TeamDetailsPage : ComponentBase
{
    [Parameter]
    public string? TeamId { get; set; }
    
    [Inject] private TabellenService TabellenService { get; set; } = null!;
    [Inject] private TeamService TeamService { get; set; } = null!;
    
    private Team Team { get; set; } = new ();
    private List<Game>? Matches { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await Init();
        await base.OnInitializedAsync();
    }
    
    private List<Player> Players = new();

    private async Task Init()
    {
        var teams = await TabellenService.GetBundesligaTabelle(2025);
        var team = teams.FirstOrDefault(x => x.Id.ToString() == TeamId);
        if(team != null)
            Team = team;
        Matches = await TeamService.GetGames(Team.Id, 3, 1);
        Matches?.Reverse();
        Matches?.RemoveAll(x => 
            x.DisplayResult == "-:-" 
            && x.MatchDateTime < DateTime.Now);
    }

    #region Helper

    private Color GetResultColor(Game game)
    {
        if (game.FinalResult == null)
            return Color.Secondary;
        if(game.FinalResult.PointsTeam1 == game.FinalResult.PointsTeam2)
            return Color.Warning;
        
        if ((game.Team1.ShortName == Team.ShortName && game.FinalResult.PointsTeam1 > game.FinalResult.PointsTeam2) || (game.Team2.ShortName == Team.ShortName && game.FinalResult.PointsTeam2 > game.FinalResult.PointsTeam1))
        {
            return Color.Success;
        }
        else
        {
            return Color.Error;
        }
    }

    #endregion

    
}