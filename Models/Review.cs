using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class Review
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Comment { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ModerationStatus { get; set; } = ReviewModerationStatuses.Pending;

    public DateTime? ModeratedOnUtc { get; set; }

    [StringLength(450)]
    public string? ModeratedByUserId { get; set; }

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
}
