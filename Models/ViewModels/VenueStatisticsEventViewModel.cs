namespace ASP_PROJECT.Models.ViewModels;

public class VenueStatisticsEventViewModel
{
    public int EventId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
    public bool IsPublished { get; init; }
    public bool HasEnded { get; init; }
    public int TicketsSold { get; init; }
    public int CheckedInTickets { get; init; }
    public int RefundedTickets { get; init; }
    public decimal GrossRevenue { get; init; }
    public decimal RefundedAmount { get; init; }
    public int ReviewCount { get; init; }
    public double AverageRating { get; init; }
}
