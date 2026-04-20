using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class Event
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1200, MinimumLength = 40)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime StartsAtUtc { get; set; }

    [Range(1, 1000)]
    public int DurationMinutes { get; set; }

    [Range(0, 1000)]
    public decimal Price { get; set; }

    [Range(5, 5000)]
    public int SeatsAvailable { get; set; }

    public bool IsPublished { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int VenueId { get; set; }
    public Venue? Venue { get; set; }

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
