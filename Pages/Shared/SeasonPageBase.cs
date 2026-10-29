using FootballTracker.Services;
using Microsoft.AspNetCore.Components;

namespace FootballTracker.Pages.Shared;

/// <summary>
/// Basis für Seiten, deren Daten von der gewählten Saison abhängen.
/// Lädt die Daten beim Start und erneut, sobald im Header eine andere Saison gewählt wird.
/// </summary>
public abstract class SeasonPageBase : ComponentBase, IDisposable
{
    [Inject] protected AppStateService AppStateService { get; set; } = null!;

    protected bool IsLoading { get; private set; } = true;
    protected string? ErrorMessage { get; private set; }
    protected int Season => AppStateService.SelectedSeason.Year;

    protected override async Task OnInitializedAsync()
    {
        AppStateService.SeasonChanged += OnSeasonChanged;
        await ReloadAsync();
    }

    protected abstract Task LoadDataAsync();

    protected Task ReloadAsync() => RunAsync(LoadDataAsync);

    /// <summary>Führt eine Ladeaktion mit Ladeanzeige und Fehlerbehandlung aus.</summary>
    protected async Task RunAsync(Func<Task> action)
    {
        IsLoading = true;
        ErrorMessage = null;
        StateHasChanged();
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void OnSeasonChanged() => _ = InvokeAsync(ReloadAsync);

    public virtual void Dispose()
    {
        AppStateService.SeasonChanged -= OnSeasonChanged;
        GC.SuppressFinalize(this);
    }
}
