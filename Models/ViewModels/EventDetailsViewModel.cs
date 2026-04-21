namespace ASP_PROJECT.Models.ViewModels;

public class EventDetailsViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Venue { get; init; } = string.Empty;
    public string VenueAddress { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public int DurationMinutes { get; init; }
    public decimal Price { get; init; }
    public int SeatsAvailable { get; init; }
    public bool HasEnded { get; init; }
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
    public IReadOnlyCollection<ReviewSummaryViewModel> Reviews { get; init; } = Array.Empty<ReviewSummaryViewModel>();
}
