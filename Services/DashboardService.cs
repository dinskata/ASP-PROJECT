using ASP_PROJECT.Services.Interfaces;
using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services;

public class DashboardService : IDashboardService
{
    private readonly IEventService _eventService;

    public DashboardService(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<DashboardViewModel> GetUserDashboardAsync(string userId, string userName)
    {
        var registrations = await _eventService.GetUserRegistrationsAsync(userId);
        var reviewables = await _eventService.GetReviewableEventsAsync(userId);
        return new DashboardViewModel
        {
            UserName = userName,
            RegistrationsCount = registrations.Count,
            ReviewsCount = reviewables.Count(x => x.HasExistingReview),
            UpcomingRegistrations = registrations
        };
    }
}
