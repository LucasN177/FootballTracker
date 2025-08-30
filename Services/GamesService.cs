using System.Net.Http.Json;
using System.Text.Json;
using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class GamesService
{
    private HttpClient _httpClient = new ();
    
    public async Task GetResults(int spieltag)
    {
        
    }

    public async Task<List<Game>> GetMatchDataAsync(string league = "bl1", int season = 2025, int matchDay = 2)
        {
                var url = $"https://api.openligadb.de/getmatchdata/{league}/{season}/{matchDay}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var games = JsonSerializer.Deserialize<List<Game>>(jsonContent, options);
                
                return games ?? new List<Game>();
            
        }

        public async Task<List<Game>> GetCurrentMatchDayAsync()
        {
            // Diese Methode könnte erweitert werden, um den aktuellen Spieltag automatisch zu ermitteln
            return await GetMatchDataAsync("bl1", 2025, GetCurrentMatchDay());
        }

        public async Task<List<Game>> GetTodaysGamesAsync()
        {
            var games = await GetCurrentMatchDayAsync();
            var today = DateTime.Today;
            
            return games.Where(g => g.MatchDateTime.Date == today).ToList();
        }

        private int GetCurrentMatchDay()
        {
            // Vereinfachte Logik - könnte durch eine separate API-Abfrage ersetzt werden
            var seasonStart = new DateTime(2025, 8, 1);
            var weeksSinceStart = (DateTime.Now - seasonStart).Days / 7;
            return Math.Max(1, Math.Min(34, (int)weeksSinceStart + 1));
        }
    }
