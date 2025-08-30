using FootballTracker.Core.Models;
using FootballTracker.Services;
using Microsoft.AspNetCore.Components;

namespace FootballTracker.Pages.Components;

public partial class TableComponent : ComponentBase
{
    
    private List<Team> _teams = new();

    protected override async Task OnInitializedAsync()
    {
        _teams = await TabellenService.GetBundesligaTabelle(AppStateService.SelectedSeason.year);
        _teams = _teams.Where(x => x.TabellenPlatz < 6).ToList();
    }
}
