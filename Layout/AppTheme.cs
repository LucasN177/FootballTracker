using FootballTracker.Core.Models;
using MudBlazor;

namespace FootballTracker.Layout;

/// <summary>MudBlazor-Theme je Wettbewerb – die Markenfarbe wechselt mit dem gewählten Wettbewerb.</summary>
public static class AppTheme
{
    private static readonly Dictionary<string, MudTheme> Themes = new();

    public static MudTheme For(Competition competition)
    {
        if (!Themes.TryGetValue(competition.Shortcut, out var theme))
        {
            theme = Create(competition);
            Themes[competition.Shortcut] = theme;
        }
        return theme;
    }

    private static MudTheme Create(Competition competition) => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = competition.PrimaryColor,
            Secondary = "#1f2937",
            Tertiary = "#7c3aed",
            Info = "#1976d2",
            Success = "#2e7d32",
            Warning = "#ed8b00",
            Error = "#d32f2f",
            Dark = "#111827",
            AppbarBackground = competition.PrimaryColor,
            AppbarText = "#ffffff",
            Background = "#f4f5f7",
            Surface = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "#1f2937",
            DrawerIcon = "#6b7280",
            TextPrimary = "#111827",
            TextSecondary = "#6b7280",
            LinesDefault = "#e5e7eb",
            TableLines = "#eef0f3",
            Divider = "#e5e7eb",
        },
        PaletteDark = new PaletteDark
        {
            Primary = competition.PrimaryLightColor,
            Secondary = "#e5e7eb",
            Tertiary = "#a78bfa",
            Info = "#4dabf5",
            Success = "#4caf50",
            Warning = "#ffb74d",
            Error = "#f44336",
            Dark = "#0b0f19",
            AppbarBackground = "#1a1d24",
            AppbarText = "#ffffff",
            Background = "#0f1115",
            Surface = "#181b21",
            DrawerBackground = "#14161b",
            DrawerText = "#e5e7eb",
            DrawerIcon = "#9ca3af",
            TextPrimary = "#f3f4f6",
            TextSecondary = "#9ca3af",
            LinesDefault = "#2a2e37",
            TableLines = "#23262e",
            Divider = "#2a2e37",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "250px",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Roboto", "Helvetica", "Arial", "sans-serif"]
            },
            H4 = new H4Typography { FontWeight = "700", LetterSpacing = "-0.02em" },
            H5 = new H5Typography { FontWeight = "700" },
            H6 = new H6Typography { FontWeight = "600" },
            Button = new ButtonTypography { FontWeight = "600", TextTransform = "none" },
            Overline = new OverlineTypography { FontWeight = "700", LetterSpacing = "0.08em" }
        }
    };
}
