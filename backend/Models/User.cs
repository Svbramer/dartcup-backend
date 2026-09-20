using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DartCupBackend.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? PasswordHash { get; set; }
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string? ProfileImageUrl { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLogin { get; set; }
    
    [Required]
    public bool IsPro { get; set; } = false;
    
    public string? GoogleId { get; set; }
    public string? FacebookId { get; set; }
    
    // Navigation properties
    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    public virtual UserStats? Stats { get; set; }
    public virtual ICollection<League> Leagues { get; set; } = new List<League>();
}

public class UserStats
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    [Required]
    public int LoginCount { get; set; } = 0;
    
    public DateTime? LastLogin { get; set; }
    
    [Required]
    public int TotalMatches { get; set; } = 0;
    
    [Required]
    public int TotalWins { get; set; } = 0;
    
    [Required]
    public int TotalScore { get; set; } = 0;
    
    [Required]
    public bool ProAccessUnlocked { get; set; } = false;
}
