using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class Registration
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Range(1, 10)]
    public int Tickets { get; set; }

    public DateTime RegisteredOnUtc { get; set; } = DateTime.UtcNow;
}
