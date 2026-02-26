using System.Text.RegularExpressions;
using FootballTracker.Core.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FootballTracker.Pages;

public partial class Tabelle : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    private void NavigateToDetailsPage(TableRowClickEventArgs<Team> args)
    {
        if(args.Item != null)
            NavigationManager.NavigateTo($"TeamDetailsPage/{args.Item.Id}");
    }
}