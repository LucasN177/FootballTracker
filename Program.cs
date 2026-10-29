using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FootballTracker;
using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Services;
using FootballTracker.Infrastructure;
using FootballTracker.Services;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using Supabase;

var culture = new CultureInfo("de-DE");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.ShowTransitionDuration = 250;
    config.SnackbarConfiguration.HideTransitionDuration = 250;
});

builder.Services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var supabaseConfig = new SupabaseConfig(builder.Configuration["Supabase:Url"], builder.Configuration["Supabase:AnonKey"]);
builder.Services.AddSingleton(supabaseConfig);
builder.Services.AddSingleton(provider =>
    new Client(
        supabaseConfig.IsConfigured ? supabaseConfig.Url : "https://not-configured.supabase.co",
        supabaseConfig.AnonKey,
        new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false,
            SessionHandler = new LocalStorageSessionHandler(provider.GetRequiredService<IJSRuntime>())
        }
    )
);

builder.Services.AddSingleton<LocalStorageService>();
builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<IDataBaseRepository, DataBaseRepository>();
builder.Services.AddSingleton<IOpenLigaDataRepository, OpenLigaDataRepository>();
builder.Services.AddSingleton<IAuthRepository, AuthRepository>();

builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<StatisticsService>();
builder.Services.AddSingleton<TabellenService>();
builder.Services.AddSingleton<TeamService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<FavoritesService>();

var host = builder.Build();

if (supabaseConfig.IsConfigured)
{
    try
    {
        // Lädt eine gespeicherte Session aus dem localStorage und erneuert sie bei Bedarf.
        await host.Services.GetRequiredService<Client>().InitializeAsync();
        await host.Services.GetRequiredService<IAuthService>().GetCurrentSession();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Supabase initialization failed: {ex.Message}");
    }
}
else
{
    Console.WriteLine("Supabase ist nicht konfiguriert – Login und Favoriten sind deaktiviert.");
}

await host.Services.GetRequiredService<AppStateService>().InitializeAsync();

await host.RunAsync();
