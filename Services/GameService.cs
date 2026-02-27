using System.Net.Http.Json;
using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class GameService
{
    private HttpClient _httpClient = new ();
    private readonly string _basicUrl = "https://api.openligadb.de/";
    public int CurrentMatchday { get; set; }

    public async Task<List<Game>?> GetMatchDataAsync(string league = "bl1", int season = 2025, int matchDay = 1)
    {
        return await _httpClient.GetFromJsonAsync<List<Game>>(_basicUrl + $"getmatchdata/{league}/{season}/{matchDay}");
    }
    

    public async Task<int> GetCurrentMatchDay()
    {
        if(CurrentMatchday != 0)
            return CurrentMatchday;
        var result = await _httpClient.GetFromJsonAsync<Group>(_basicUrl + "getcurrentgroup/bl1");
        CurrentMatchday = result?.GroupOrderId ?? 0;
        return CurrentMatchday;
    }
    
}