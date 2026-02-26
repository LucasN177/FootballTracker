using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FootballTracker;
using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Infrastructure;
using FootballTracker.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<AppStateService>();

builder.Services.AddSingleton<IOpenLigaDataRepository, OpenLigaDataRepository>();

builder.Services.AddSingleton<TabellenService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<TeamService>();


await builder.Build().RunAsync();
