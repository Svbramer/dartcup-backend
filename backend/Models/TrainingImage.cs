using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DartCupBackend.Models;

public class TrainingImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public bool IsProOnly { get; set; } = true;
    
    public Guid? UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    // For screenshots of high scores
    public int? HighScore { get; set; }
    public string? GameType { get; set; } // 501, 301, Cricket, etc.
    public DateTime? AchievedAt { get; set; }
}
