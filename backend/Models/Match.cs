using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DartCupBackend.Models;

public class Match
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid Player1Id { get; set; }
    
    [ForeignKey("Player1Id")]
    public virtual Player Player1 { get; set; } = null!;
    
    [Required]
    public Guid Player2Id { get; set; }
    
    [ForeignKey("Player2Id")]
    public virtual Player Player2 { get; set; } = null!;
    
    public Guid? WinnerId { get; set; }
    
    [ForeignKey("WinnerId")]
    public virtual Player? Winner { get; set; }
    
    [Required]
    public int ScorePlayer1 { get; set; } = 0;
    
    [Required]
    public int ScorePlayer2 { get; set; } = 0;
    
    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    public Guid? LeagueId { get; set; }
    
    [ForeignKey("LeagueId")]
    public virtual League? League { get; set; }
    
    [Required]
    public string GameType { get; set; } = "501"; // 501, 301, 201, Cricket, Skovhugger
    
    public bool IsCompleted { get; set; } = false;
    
    public string? Notes { get; set; }
}
