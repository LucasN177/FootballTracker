using FootballTracker.Core.Models;
using FootballTracker.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FootballTracker.Pages.Components;

public partial class Ergebnisse : ComponentBase
{
    [Inject] private GameService GameService { get; set; } = null!;
    
    private int MatchDay { get; set; }
    private List<Game>? AllGames { get; set; } = new();
    private List<Game> TodaysGames => AllGames!.Where(IsToday).ToList();
    private List<Game> UpcomingGames => AllGames!.Where(IsUpcoming).ToList();
    private List<Game> FinishedGames => AllGames!.Where(g => g.MatchIsFinished).ToList();

    private bool IsLoading { get; set; } = true;
    private int ActiveTabIndex { get; set; } = 0;
    private int SelectedMatchDay { get; set; }
    private DateTime LastUpdateTime { get; set; } = DateTime.Now;
    
    protected override async Task OnInitializedAsync()
    {
        MatchDay = await GameService.GetCurrentMatchDay();
        SelectedMatchDay = MatchDay;
        await LoadMatchDay();
    }
    
    private async Task LoadMatchDay()
    {
        IsLoading = true;
        try
        {
            AllGames = await GameService.GetMatchDataAsync("bl1", 2025, SelectedMatchDay);
            LastUpdateTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task RefreshData()
    {
        await LoadMatchDay();
    }

    private string GetTabText(string tabType, int count)
    {
        return tabType switch
        {
            "today" => $"Heute ({count})",
            "upcoming" => $"Anstehend ({count})",
            "finished" => $"Beendet ({count})",
            "all" => $"Alle ({count})",
            _ => $"Tab ({count})"
        };
    }

    private (string Text, Color Color, Variant Variant) GetGameStatus(Game game)
    {
        if (game.MatchIsFinished)
            return ("Beendet", Color.Success, Variant.Filled);

        if (game.MatchDateTime > DateTime.Now)
            return ("Angesetzt", Color.Info, Variant.Outlined);

        return ("Live", Color.Error, Variant.Filled);
    }

    private string GetScoreDisplay(Game game)
    {
        if (game.FinalResult != null)
            return $"{game.FinalResult.PointsTeam1} : {game.FinalResult.PointsTeam2}";

        return game.IsUpcoming ? game.MatchDateTime.ToString("HH:mm") : "- : -";
    }

    private string GetGoalIcon(Goal goal)
    {
        if (goal.IsOwnGoal) return Icons.Material.Filled.SportsSoccer;
        return goal.IsPenalty ? Icons.Material.Filled.GpsFixed : Icons.Material.Filled.SportsSoccer;
    }

    private bool IsToday(Game game) => game.MatchDateTime.Date == DateTime.Today;
    private bool IsUpcoming(Game game) => !game.MatchIsFinished && game.MatchDateTime > DateTime.Now;
    
    
}