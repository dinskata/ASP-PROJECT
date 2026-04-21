using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    [StringLength(60)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string ActionType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [StringLength(450)]
    public string? PerformedByUserId { get; set; }

    [Required]
    [StringLength(120)]
    public string PerformedByName { get; set; } = "System";

    [Required]
    [StringLength(300)]
    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
}
