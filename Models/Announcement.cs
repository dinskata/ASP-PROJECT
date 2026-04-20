using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class Announcement
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 20)]
    public string Content { get; set; } = string.Empty;

    [StringLength(80)]
    public string Audience { get; set; } = "All";

    public DateTime PublishedOnUtc { get; set; } = DateTime.UtcNow;

    public bool IsPinned { get; set; }
}
