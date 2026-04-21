namespace ASP_PROJECT.Models.ViewModels;

public class VenueListItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public int EventCount { get; init; }
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}
