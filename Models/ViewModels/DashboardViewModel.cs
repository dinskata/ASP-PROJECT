namespace ASP_PROJECT.Models.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; init; } = string.Empty;
    public int RegistrationsCount { get; init; }
    public int ReviewsCount { get; init; }
    public IReadOnlyCollection<RegistrationSummaryViewModel> UpcomingRegistrations { get; init; } = Array.Empty<RegistrationSummaryViewModel>();
}
