namespace FootballTracker.Services;

public class AppStateService
{
    public bool IsDarkMode { get; private set; }
    public string SelectedSeason { get; set; } = "2025/26";

    public bool IsDrawerOpen { get; set; } = true;

    public event Action? OnChange;

    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        NotifyStateChanged();
    }

    public void SetSeason(string season)
    {
        SelectedSeason = season;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
    
    public void DrawerToggle()
    {
        IsDrawerOpen = !IsDrawerOpen;
    }
}