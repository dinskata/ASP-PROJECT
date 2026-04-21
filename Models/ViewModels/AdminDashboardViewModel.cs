namespace ASP_PROJECT.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int EventsCount { get; init; }
    public int VenuesCount { get; init; }
    public int RegistrationsCount { get; init; }
    public int UsersCount { get; init; }
    public int PendingReviewsCount { get; init; }
    public int RefundedPaymentsCount { get; init; }
    public int VenueManagersCount { get; init; }
    public int SiteModeratorsCount { get; init; }
    public IReadOnlyCollection<EventListItemViewModel> NextEvents { get; init; } = Array.Empty<EventListItemViewModel>();
    public IReadOnlyCollection<AuditLogListItemViewModel> RecentAuditEntries { get; init; } = Array.Empty<AuditLogListItemViewModel>();
}
