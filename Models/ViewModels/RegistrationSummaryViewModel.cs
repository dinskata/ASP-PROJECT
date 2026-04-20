namespace ASP_PROJECT.Models.ViewModels;

public class RegistrationSummaryViewModel
{
    public int RegistrationId { get; init; }
    public int EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public int Tickets { get; init; }
    public string Venue { get; init; } = string.Empty;
}
