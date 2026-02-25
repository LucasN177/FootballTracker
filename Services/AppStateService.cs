using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class AppStateService
{
    public static List<Season> Saisons { get; set; } = new ()
    {
        new Season()
        {
            Name = "2025/2026",
            Year = 2025
        },
        new Season()
        {
            Name = "2024/2025",
            Year = 2024
        },
        new Season()
        {
            Name = "2023/2024",
            Year = 2023
        },
        new Season()
        {
            Name = "2022/2023",
            Year = 2022
        }
    };
    public bool IsDarkMode { get; private set; }
    public Season SelectedSeason { get; set; }

    public bool IsDrawerOpen { get; set; } = true;

    public AppStateService()
    {
        SelectedSeason = Saisons[0];
    }
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