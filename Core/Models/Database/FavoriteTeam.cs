using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballTracker.Core.Models.Database;

[Table("favorite_teams")]
public class FavoriteTeam : BaseModel
{
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("team_name")]
    public string? TeamName { get; set; }

    [Column("team_icon_url")]
    public string? TeamIconUrl { get; set; }
}
