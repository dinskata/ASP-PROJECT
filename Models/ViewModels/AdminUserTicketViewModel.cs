namespace ASP_PROJECT.Models.ViewModels;

public class AdminUserTicketViewModel
{
    public int TicketId { get; init; }
    public int RegistrationId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public string VenueName { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public string VerificationCode { get; init; } = string.Empty;
    public string SeatLabel { get; init; } = string.Empty;
    public string TicketNote { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public bool CanEdit { get; init; }
}
