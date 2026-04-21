namespace ASP_PROJECT.Models.ViewModels;

public class ManagementEventListItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Venue { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public bool IsPublished { get; init; }
    public int TicketsSold { get; init; }
    public int ReviewCount { get; init; }
}
