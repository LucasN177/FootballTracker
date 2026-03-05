using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FootballTracker;
using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Services;
using FootballTracker.Infrastructure;
using FootballTracker.Services;
using MudBlazor.Services;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });



builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<IDataBaseRepository, DataBaseRepository>();
builder.Services.AddSingleton<IOpenLigaDataRepository, OpenLigaDataRepository>();
builder.Services.AddSingleton<IAuthRepository, AuthRepository>();

builder.Services.AddSingleton<TabellenService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<TeamService>();
builder.Services.AddSingleton<IAuthService, AuthService>();

var url = builder.Configuration["Supabase:Url"];
var key = builder.Configuration["Supabase:AnonKey"];

var options = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true
};

builder.Services.AddSingleton<Client>(provider =>
    new Client(
        url!, key, options
    )
);

await builder.Build().RunAsync();
