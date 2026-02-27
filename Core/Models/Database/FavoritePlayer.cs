using System.ComponentModel.DataAnnotations.Schema;
using Supabase.Postgrest.Models;

namespace FootballTracker.Core.Models.Database;

[Table("user_favorite_players")]
public class FavoritePlayer : BaseModel
{
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("player_id")]
    public int PlayerId { get; set; }
}