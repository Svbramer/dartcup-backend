using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DartCupBackend.Models;

public class ScoreEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid RoomId { get; set; }

    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; } = null!;

    [Required]
    public Guid PlayerId { get; set; }

    [Required]
    public int VisitNumber { get; set; } // 1-3 kast i samme omgang

    [Required]
    public int Value { get; set; }

    [Required]
    public string Status { get; set; } = "pending"; // pending (3 sek. rettelsesvindue), confirmed

    [Required]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ConfirmedAt { get; set; }
}
