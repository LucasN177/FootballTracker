using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Interfaces.Services;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Database;

namespace FootballTracker.Services;

/// <summary>Hält die Favoriten des eingeloggten Nutzers im Speicher und synchronisiert sie mit Supabase.</summary>
public class FavoritesService
{
    private readonly IDataBaseRepository _dataBaseRepository;
    private readonly IAuthService _authService;
    private Task? _loadTask;

    public FavoritesService(IDataBaseRepository dataBaseRepository, IAuthService authService)
    {
        _dataBaseRepository = dataBaseRepository;
        _authService = authService;
        _authService.AuthStateChanged += () => _ = ReloadAsync();
    }

    public event Action? Changed;

    public List<FavoritePlayer> Players { get; private set; } = new();
    public List<FavoriteTeam> Teams { get; private set; } = new();
    public string? LastError { get; private set; }
    public bool IsLoggedIn => _authService.IsLoggedIn;

    public Task EnsureLoadedAsync() => _loadTask ??= ReloadAsync();

    public async Task ReloadAsync()
    {
        LastError = null;
        if (!_authService.IsLoggedIn)
        {
            Players = new();
            Teams = new();
        }
        else
        {
            var playersTask = _dataBaseRepository.GetFavoritePlayers();
            var teamsTask = _dataBaseRepository.GetFavoriteTeams();
            var players = await playersTask;
            var teams = await teamsTask;

            Players = players.Data ?? new();
            Teams = teams.Data ?? new();
            LastError = players.IsSuccess && teams.IsSuccess
                ? null
                : players.ErrorMessage ?? players.Exception?.Message ?? teams.ErrorMessage ?? teams.Exception?.Message;
        }
        Changed?.Invoke();
    }

    public bool IsFavoritePlayer(int playerId) => Players.Any(p => p.PlayerId == playerId);

    public bool IsFavoriteTeam(int teamId) => Teams.Any(t => t.TeamId == teamId);

    public async Task<IResponse> TogglePlayerAsync(int playerId, string playerName)
    {
        if (!_authService.IsLoggedIn)
            return NotLoggedIn();

        var isFavorite = IsFavoritePlayer(playerId);
        var result = isFavorite
            ? await _dataBaseRepository.DeletePlayerFromFavorites(playerId)
            : await _dataBaseRepository.AddPlayerToFavorites(new Player { Id = playerId, Name = playerName });

        if (result.IsSuccess)
        {
            if (isFavorite)
                Players.RemoveAll(p => p.PlayerId == playerId);
            else
                Players.Add(new FavoritePlayer { PlayerId = playerId, PlayerName = playerName, UserId = _authService.CurrentUser?.Id ?? string.Empty });
            Changed?.Invoke();
        }
        return result;
    }

    public async Task<IResponse> ToggleTeamAsync(Team team)
    {
        if (!_authService.IsLoggedIn)
            return NotLoggedIn();

        var isFavorite = IsFavoriteTeam(team.Id);
        var result = isFavorite
            ? await _dataBaseRepository.DeleteTeamFromFavorites(team.Id)
            : await _dataBaseRepository.AddTeamToFavorites(team);

        if (result.IsSuccess)
        {
            if (isFavorite)
                Teams.RemoveAll(t => t.TeamId == team.Id);
            else
                Teams.Add(new FavoriteTeam { TeamId = team.Id, TeamName = team.Name, TeamIconUrl = team.LogoUrl, UserId = _authService.CurrentUser?.Id ?? string.Empty });
            Changed?.Invoke();
        }
        return result;
    }

    private static IResponse NotLoggedIn() =>
        Response.Failure("Bitte melde dich an, um Favoriten zu speichern.", null);
}
