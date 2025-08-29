using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class AppStateService
{
    public static List<Season> Saisons { get; set; } = new ()
    {
        new Season()
        {
            Name = "2025/2026",
            year = 2025
        },
        new Season()
        {
            Name = "2024/2025",
            year = 2024
        },
        new Season()
        {
            Name = "2023/2024",
            year = 2023
        },
        new Season()
        {
            Name = "2022/2023",
            year = 2022
        }
    };
    public bool IsDarkMode { get; private set; }
    public Season SelectedSeason { get; set; } = Saisons[0];

    public bool IsDrawerOpen { get; set; } = true;

    public event Action? OnChange;

    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        NotifyStateChanged();
    }

    public void SetSeason(Season season)
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