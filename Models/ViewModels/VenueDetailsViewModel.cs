namespace ASP_PROJECT.Models.ViewModels;

public class VenueDetailsViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public string Description { get; init; } = string.Empty;
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
    public IReadOnlyCollection<EventListItemViewModel> UpcomingEvents { get; init; } = Array.Empty<EventListItemViewModel>();
    public IReadOnlyCollection<EventListItemViewModel> EndedEvents { get; init; } = Array.Empty<EventListItemViewModel>();
}
