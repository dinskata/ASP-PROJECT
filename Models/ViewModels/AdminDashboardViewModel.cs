namespace ASP_PROJECT.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int EventsCount { get; init; }
    public int VenuesCount { get; init; }
    public int RegistrationsCount { get; init; }
    public int UsersCount { get; init; }
    public IReadOnlyCollection<EventListItemViewModel> NextEvents { get; init; } = Array.Empty<EventListItemViewModel>();
}
