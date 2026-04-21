namespace ASP_PROJECT.Models.ViewModels;

public class ManagerDashboardViewModel
{
    public string ManagerName { get; init; } = string.Empty;
    public int AssignedVenuesCount { get; init; }
    public int ManagedEventsCount { get; init; }
    public IReadOnlyCollection<VenueListItemViewModel> AssignedVenues { get; init; } = Array.Empty<VenueListItemViewModel>();
    public IReadOnlyCollection<ManagementEventListItemViewModel> ManagedEvents { get; init; } = Array.Empty<ManagementEventListItemViewModel>();
}
