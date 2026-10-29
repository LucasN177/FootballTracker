using FootballTracker.Services;
using Microsoft.AspNetCore.Components;

namespace FootballTracker.Pages.Shared;

/// <summary>
/// Basis für Seiten, deren Daten von der gewählten Saison abhängen.
/// Lädt die Daten beim Start und erneut, sobald im Header eine andere Saison oder ein anderer Wettbewerb gewählt wird.
/// </summary>
public abstract class SeasonPageBase : ComponentBase, IDisposable
{
    [Inject] protected AppStateService AppStateService { get; set; } = null!;

    protected bool IsLoading { get; private set; } = true;
    protected string? ErrorMessage { get; private set; }
    protected int Season => AppStateService.SelectedSeason.Year;

    protected override async Task OnInitializedAsync()
    {
        AppStateService.SelectionChanged += OnSelectionChanged;
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

    private void OnSelectionChanged() => _ = InvokeAsync(ReloadAsync);

    public virtual void Dispose()
    {
        AppStateService.SelectionChanged -= OnSelectionChanged;
        GC.SuppressFinalize(this);
    }
}
