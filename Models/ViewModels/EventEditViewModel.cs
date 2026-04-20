using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class EventEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1200, MinimumLength = 40)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Start Date and Time")]
    public DateTime StartsAtUtc { get; set; } = DateTime.UtcNow.AddDays(7);

    [Range(30, 1000)]
    [Display(Name = "Duration (minutes)")]
    public int DurationMinutes { get; set; }

    [Range(0, 1000)]
    public decimal Price { get; set; }

    [Range(5, 5000)]
    [Display(Name = "Seats Available")]
    public int SeatsAvailable { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; }

    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Venue")]
    public int VenueId { get; set; }

    public IReadOnlyCollection<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyCollection<Venue> Venues { get; set; } = Array.Empty<Venue>();
}
