
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FootballTracker.Core.Models.Database;

[Table("favorite_players")]
public class FavoritePlayer : BaseModel
{
    [Column("user_id")] 
    public string UserId { get; set; } = string.Empty;
    
    [Column("player_id")]
    public int PlayerId { get; set; }
}