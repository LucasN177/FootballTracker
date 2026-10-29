namespace FootballTracker.Services;

/// <summary>
/// Supabase-Zugangsdaten aus der appsettings.json. Im Repository stehen dort nur Platzhalter,
/// die beim Deployment durch die GitHub-Secrets SUPABASE_URL und SUPABASE_KEY ersetzt werden.
/// </summary>
public class SupabaseConfig(string? url, string? anonKey)
{
    public string Url { get; } = url ?? string.Empty;
    public string AnonKey { get; } = anonKey ?? string.Empty;

    public bool IsConfigured =>
        Uri.TryCreate(Url, UriKind.Absolute, out var uri)
        && uri.Scheme.StartsWith("http")
        && !string.IsNullOrWhiteSpace(AnonKey)
        && AnonKey != "SUPABASE_KEY";
}
