using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class UserVenueAssignment
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int VenueId { get; set; }
    public Venue? Venue { get; set; }

    public DateTime AssignedOnUtc { get; set; } = DateTime.UtcNow;
}
