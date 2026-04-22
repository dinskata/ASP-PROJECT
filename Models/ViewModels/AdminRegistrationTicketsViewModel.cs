namespace ASP_PROJECT.Models.ViewModels;

public class AdminRegistrationTicketsViewModel
{
    public int RegistrationId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string BuyerName { get; init; } = string.Empty;
    public string EventTitle { get; init; } = string.Empty;
    public string VenueName { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public int TicketsCount { get; init; }
    public decimal AmountPaid { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string CardLast4 { get; init; } = string.Empty;
    public DateTime RegisteredOnUtc { get; init; }
    public IReadOnlyCollection<AdminUserTicketViewModel> Tickets { get; init; } = Array.Empty<AdminUserTicketViewModel>();
}
