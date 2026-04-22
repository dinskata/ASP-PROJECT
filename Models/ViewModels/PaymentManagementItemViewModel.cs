namespace ASP_PROJECT.Models.ViewModels;

public class PaymentManagementItemViewModel
{
    public int RegistrationId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public int EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public string BuyerName { get; init; } = string.Empty;
    public string VenueName { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public int Tickets { get; init; }
    public decimal AmountPaid { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string CardLast4 { get; init; } = string.Empty;
    public string PrimaryTicketCode { get; init; } = string.Empty;
    public DateTime RegisteredOnUtc { get; init; }
    public DateTime? RefundedOnUtc { get; init; }
    public bool CanForceRefund { get; init; }
}
