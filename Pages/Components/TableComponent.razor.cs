using FootballTracker.Core.Models;
using FootballTracker.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FootballTracker.Pages.Components;

public partial class TableComponent : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    private List<Team> _teams = new();

    protected override async Task OnInitializedAsync()
    {
        _teams = await TabellenService.GetBundesligaTabelle(AppStateService.SelectedSeason.Year);
        _teams = _teams.Where(x => x.TabellenPlatz < 6).ToList();
    }
    
    private void NavigateToDetailsPage(TableRowClickEventArgs<Team> args)
    {
        if(args.Item != null)
            NavigationManager.NavigateTo($"TeamDetailsPage/{args.Item.Id}");
    }
}
