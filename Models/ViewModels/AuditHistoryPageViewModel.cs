namespace ASP_PROJECT.Models.ViewModels;

public class AuditHistoryPageViewModel
{
    public IReadOnlyCollection<AuditLogListItemViewModel> Entries { get; init; } = Array.Empty<AuditLogListItemViewModel>();
    public IReadOnlyCollection<TicketRegistryItemViewModel> Tickets { get; init; } = Array.Empty<TicketRegistryItemViewModel>();
}
