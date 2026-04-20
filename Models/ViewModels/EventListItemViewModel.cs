namespace ASP_PROJECT.Models.ViewModels;

public class EventListItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Venue { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public decimal Price { get; init; }
    public int SeatsAvailable { get; init; }
    public double AverageRating { get; init; }
}
