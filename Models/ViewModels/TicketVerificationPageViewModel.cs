namespace ASP_PROJECT.Models.ViewModels;

public class TicketVerificationPageViewModel
{
    public string TicketCode { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public TicketVerificationResultViewModel? Result { get; init; }
    public IReadOnlyCollection<TicketRegistryItemViewModel> DemoTickets { get; init; } = Array.Empty<TicketRegistryItemViewModel>();
    public IReadOnlyCollection<TicketRegistryItemViewModel> RecentTickets { get; init; } = Array.Empty<TicketRegistryItemViewModel>();
}
