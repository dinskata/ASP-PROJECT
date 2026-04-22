namespace ASP_PROJECT.Models.ViewModels;

public class TicketRegistryItemViewModel
{
    public int RegistrationId { get; init; }
    public int EventId { get; init; }
    public string TicketCode { get; init; } = string.Empty;
    public string VerificationCode { get; init; } = string.Empty;
    public string EventTitle { get; init; } = string.Empty;
    public string BuyerName { get; init; } = string.Empty;
    public string VenueName { get; init; } = string.Empty;
    public string SeatLabel { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public DateTime RegisteredOnUtc { get; init; }
    public bool IsCheckedIn { get; init; }
    public DateTime? CheckedInOnUtc { get; init; }
    public string CheckedInByName { get; init; } = string.Empty;
}
