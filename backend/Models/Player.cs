using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DartCupBackend.Models;

public class Player
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? AvatarUrl { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Match> MatchesAsPlayer1 { get; set; } = new List<Match>();
    public virtual ICollection<Match> MatchesAsPlayer2 { get; set; } = new List<Match>();
    public virtual ICollection<Match> MatchesWon { get; set; } = new List<Match>();
    public virtual ICollection<LeagueStanding> LeagueStandings { get; set; } = new List<LeagueStanding>();
}
