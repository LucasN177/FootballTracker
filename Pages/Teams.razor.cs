using FootballTracker.Core.Models;
using FootballTracker.Services;
using Microsoft.AspNetCore.Components;

namespace FootballTracker.Pages;

public partial class Teams : ComponentBase
{
    [Inject] private AppStateService AppStateService { get; init; } = null!;
    [Inject] private TabellenService TabellenService { get; init; } = null!;
    List<Team> _teams = new();

    protected override async Task OnInitializedAsync()
    {
        await GetTeams(AppStateService.SelectedSeason.Year);
    }

    private async Task GetTeams(int yearLocal = 2025)
    {
        _teams = await TabellenService.GetBundesligaTabelle(yearLocal);
        
        await InvokeAsync(StateHasChanged);
    }
}