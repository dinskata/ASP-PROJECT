namespace ASP_PROJECT.Models.ViewModels;

public class AdminUserDetailsViewModel
{
    public string UserId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string RolesDisplay { get; init; } = string.Empty;
    public DateTime CreatedOnUtc { get; init; }
    public IReadOnlyCollection<PaymentManagementItemViewModel> Purchases { get; init; } = Array.Empty<PaymentManagementItemViewModel>();
    public IReadOnlyCollection<AdminUserTicketViewModel> ActiveTickets { get; init; } = Array.Empty<AdminUserTicketViewModel>();
}
