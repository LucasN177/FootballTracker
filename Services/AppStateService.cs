using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class AppStateService(LocalStorageService localStorage)
{
    private const string DarkModeKey = "ft.darkMode";
    private const string SeasonKey = "ft.season";
    private const string CompetitionKey = "ft.competition";
    private const int FirstSeasonYear = 2020;

    /// <summary>Die laufende Saison startet im Juli.</summary>
    public static int CurrentSeasonYear => DateTime.Today.Month >= 7 ? DateTime.Today.Year : DateTime.Today.Year - 1;

    private static readonly List<Season> AllSeasons = Enumerable
        .Range(FirstSeasonYear, CurrentSeasonYear - FirstSeasonYear + 1)
        .Reverse()
        .Select(Season.FromYear)
        .ToList();

    /// <summary>Saisons, für die der gewählte Wettbewerb Daten hat (neueste zuerst).</summary>
    public List<Season> Saisons => AllSeasons.Where(s => s.Year >= SelectedCompetition.FirstSeason).ToList();

    public bool IsDarkMode { get; private set; }
    public Season SelectedSeason { get; private set; } = AllSeasons[0];
    public Competition SelectedCompetition { get; private set; } = Competition.Bundesliga;
    public bool IsCurrentSeason => SelectedSeason.Year == CurrentSeasonYear;

    /// <summary>OpenLigaDB-Kürzel des gewählten Wettbewerbs, z.B. "bl1".</summary>
    public string League => SelectedCompetition.Shortcut;

    public bool IsDrawerOpen { get; set; } = true;

    /// <summary>Allgemeine Zustandsänderung (Theme, Drawer, ...).</summary>
    public event Action? OnChange;

    /// <summary>Wird ausgelöst, wenn Saison oder Wettbewerb gewechselt wurden – Seiten laden dann ihre Daten neu.</summary>
    public event Action? SelectionChanged;

    public async Task InitializeAsync()
    {
        IsDarkMode = await localStorage.GetAsync(DarkModeKey) == "true";

        var competition = await localStorage.GetAsync(CompetitionKey);
        SelectedCompetition = Competition.All.FirstOrDefault(c => c.Shortcut == competition) ?? Competition.Bundesliga;

        if (int.TryParse(await localStorage.GetAsync(SeasonKey), out var year))
            SelectedSeason = FindSeason(year);
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
        var selected = FindSeason(season.Year);
        if (selected == SelectedSeason)
            return;

        SelectedSeason = selected;
        NotifySelectionChanged();
        await localStorage.SetAsync(SeasonKey, selected.Year.ToString());
    }

    public async Task SetCompetition(Competition competition)
    {
        if (competition == SelectedCompetition)
            return;

        SelectedCompetition = competition;
        // Nicht jeder Wettbewerb hat Daten für alle Saisons
        SelectedSeason = FindSeason(SelectedSeason.Year);
        NotifySelectionChanged();
        await localStorage.SetAsync(CompetitionKey, competition.Shortcut);
    }

    private Season FindSeason(int year) => Saisons.FirstOrDefault(s => s.Year == year) ?? Saisons[0];

    private void NotifySelectionChanged()
    {
        NotifyStateChanged();
        SelectionChanged?.Invoke();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void DrawerToggle()
    {
        IsDrawerOpen = !IsDrawerOpen;
        NotifyStateChanged();
    }
}
