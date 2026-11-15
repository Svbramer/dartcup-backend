using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DartCupBackend.Models;

public class RoomSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid RoomId { get; set; }

    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; } = null!;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string DeviceName { get; set; } = string.Empty;

    [Required]
    public string Roles { get; set; } = "viewer"; // csv: scorer, view-opponent, camera-board, camera-person

    [Required]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
