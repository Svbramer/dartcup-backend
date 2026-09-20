using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DartCupBackend.Models;

public class League
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Guid OwnerId { get; set; }
    
    [ForeignKey("OwnerId")]
    public virtual User Owner { get; set; } = null!;
    
    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    public virtual ICollection<LeagueStanding> Standings { get; set; } = new List<LeagueStanding>();
}

public class LeagueStanding
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid LeagueId { get; set; }
    
    [ForeignKey("LeagueId")]
    public virtual League League { get; set; } = null!;
    
    [Required]
    public Guid PlayerId { get; set; }
    
    [ForeignKey("PlayerId")]
    public virtual Player Player { get; set; } = null!;
    
    [Required]
    public int Position { get; set; } = 1;
    
    [Required]
    public int Points { get; set; } = 0;
    
    [Required]
    public int Wins { get; set; } = 0;
    
    [Required]
    public int Losses { get; set; } = 0;
    
    public int? WeeklyRank { get; set; }
    
    public int? MonthlyRank { get; set; }
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
