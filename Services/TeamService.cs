using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class TeamService(IOpenLigaDataRepository openLigaDataRepository, TabellenService tabellenService)
{
    /// <summary>
    /// Liefert ein Team der Saison inkl. Tabellenwerten. Teams, die (noch) nicht in der Tabelle stehen,
    /// werden aus der Liste der verfügbaren Teams ergänzt.
    /// </summary>
    public async Task<Team?> GetTeamAsync(int season, int teamId)
    {
        var table = await tabellenService.GetBundesligaTabelle(season);
        var team = table.FirstOrDefault(t => t.Id == teamId);
        if (team != null)
            return team;

        var teams = await GetAllTeams(AppStateService.LeagueShortcut, season);
        return teams.FirstOrDefault(t => t.Id == teamId);
    }

    public async Task<List<Team>> GetAllTeams(string leagueShortcut, int year)
    {
        var result = await openLigaDataRepository.GetTeams(leagueShortcut, year);
        if (result is not { IsSuccess: true, Data: not null })
            return new List<Team>();

        return result.Data
            .Select(t => new Team
            {
                Name = t.teamName,
                ShortName = t.shortName,
                LogoUrl = t.teamIconUrl,
                Id = t.teamInfoId,
            })
            .OrderBy(t => t.Name)
            .ToList();
    }
}
