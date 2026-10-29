using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class AppStateService(LocalStorageService localStorage)
{
    private const string DarkModeKey = "ft.darkMode";
    private const string SeasonKey = "ft.season";
    private const int FirstSeasonYear = 2020;

    public const string LeagueShortcut = "bl1";

    /// <summary>Die laufende Saison startet im Juli.</summary>
    public static int CurrentSeasonYear => DateTime.Today.Month >= 7 ? DateTime.Today.Year : DateTime.Today.Year - 1;

    public static List<Season> Saisons { get; } = Enumerable
        .Range(FirstSeasonYear, CurrentSeasonYear - FirstSeasonYear + 1)
        .Reverse()
        .Select(Season.FromYear)
        .ToList();

    public bool IsDarkMode { get; private set; }
    public Season SelectedSeason { get; private set; } = Saisons[0];
    public bool IsCurrentSeason => SelectedSeason.Year == CurrentSeasonYear;

    public bool IsDrawerOpen { get; set; } = true;

    /// <summary>Allgemeine Zustandsänderung (Theme, Drawer, ...).</summary>
    public event Action? OnChange;

    /// <summary>Wird ausgelöst, wenn eine andere Saison gewählt wurde – Seiten laden dann ihre Daten neu.</summary>
    public event Action? SeasonChanged;

    public async Task InitializeAsync()
    {
        IsDarkMode = await localStorage.GetAsync(DarkModeKey) == "true";

        if (int.TryParse(await localStorage.GetAsync(SeasonKey), out var year))
            SelectedSeason = Saisons.FirstOrDefault(s => s.Year == year) ?? Saisons[0];
    }

    public async Task ToggleDarkMode() => await SetDarkMode(!IsDarkMode);

    public async Task SetDarkMode(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        NotifyStateChanged();
        await localStorage.SetAsync(DarkModeKey, isDarkMode ? "true" : "false");
    }

    public async Task SetSeason(Season season)
    {
        var selected = Saisons.FirstOrDefault(s => s.Year == season.Year) ?? Saisons[0];
        if (selected == SelectedSeason)
            return;

        SelectedSeason = selected;
        NotifyStateChanged();
        SeasonChanged?.Invoke();
        await localStorage.SetAsync(SeasonKey, selected.Year.ToString());
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void DrawerToggle()
    {
        IsDrawerOpen = !IsDrawerOpen;
        NotifyStateChanged();
    }
}
