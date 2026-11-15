using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DartCupBackend.Models;

public class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [StringLength(6)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string GameType { get; set; } = "201"; // 201, 301, 501, Cricket, Skovhugger

    [Required]
    public Guid Player1Id { get; set; }

    public Guid? Player2Id { get; set; }

    [Required]
    public string Status { get; set; } = "waiting"; // waiting, active, finished

    [Required]
    public int CurrentTurn { get; set; } = 1; // 1 = Player1, 2 = Player2

    public Guid? BoardHolderSessionId { get; set; }

    public Guid? BoardRequestFromSessionId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<RoomSession> Sessions { get; set; } = new List<RoomSession>();
    public virtual ICollection<ScoreEntry> ScoreEntries { get; set; } = new List<ScoreEntry>();
}
