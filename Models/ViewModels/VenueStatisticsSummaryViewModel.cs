namespace ASP_PROJECT.Models.ViewModels;

public class VenueStatisticsSummaryViewModel
{
    public int VenueId { get; init; }
    public string VenueName { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public int TotalEvents { get; init; }
    public int PublishedEvents { get; init; }
    public int UpcomingEvents { get; init; }
    public int EndedEvents { get; init; }
    public int TicketsSold { get; init; }
    public int CheckedInTickets { get; init; }
    public int RefundedTickets { get; init; }
    public decimal GrossRevenue { get; init; }
    public decimal RefundedAmount { get; init; }
    public int ApprovedReviewsCount { get; init; }
    public double AverageRating { get; init; }
    public double CheckInRatePercent { get; init; }
    public double RefundRatePercent { get; init; }
    public string TopEventTitle { get; init; } = string.Empty;
    public string BestRatedEventTitle { get; init; } = string.Empty;
    public IReadOnlyCollection<VenueStatisticsEventViewModel> Events { get; init; } = Array.Empty<VenueStatisticsEventViewModel>();
}
