using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FootballTracker;
using FootballTracker.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<TabellenService>();
builder.Services.AddSingleton<GamesService>();
builder.Services.AddSingleton<TeamService>();
//builder.Services.AddSingleton<NavigationManager>();

await builder.Build().RunAsync();
