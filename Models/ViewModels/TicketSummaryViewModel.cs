namespace ASP_PROJECT.Models.ViewModels;

public class TicketSummaryViewModel
{
    public int TicketId { get; init; }
    public int TicketNumber { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public string VerificationCode { get; init; } = string.Empty;
    public string SeatLabel { get; init; } = string.Empty;
    public string TicketNote { get; init; } = string.Empty;
    public bool IsCheckedIn { get; init; }
    public DateTime? CheckedInOnUtc { get; init; }
    public string CheckedInByName { get; init; } = string.Empty;
}
