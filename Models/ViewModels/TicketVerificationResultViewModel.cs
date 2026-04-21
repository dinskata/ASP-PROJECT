namespace ASP_PROJECT.Models.ViewModels;

public class TicketVerificationResultViewModel
{
    public bool IsSubmitted { get; init; }
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
    public string TicketCode { get; init; } = string.Empty;
    public string VerificationCode { get; init; } = string.Empty;
    public string EventTitle { get; init; } = string.Empty;
    public string VenueName { get; init; } = string.Empty;
    public string BuyerName { get; init; } = string.Empty;
    public string SeatLabel { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public DateTime? StartsAtUtc { get; init; }
}
