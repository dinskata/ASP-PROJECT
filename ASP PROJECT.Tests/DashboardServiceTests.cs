using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services;
using ASP_PROJECT.Services.Interfaces;
using Xunit;

namespace ASP_PROJECT.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetUserDashboardAsync_UsesRegistrationsFromEventService()
    {
        var registrations = new[]
        {
            new RegistrationSummaryViewModel { RegistrationId = 1, EventId = 11, EventTitle = "Event A", Tickets = 1, Venue = "Hall A", StartsAtUtc = DateTime.UtcNow.AddDays(1) },
            new RegistrationSummaryViewModel { RegistrationId = 2, EventId = 12, EventTitle = "Event B", Tickets = 2, Venue = "Hall B", StartsAtUtc = DateTime.UtcNow.AddDays(2) }
        };

        var service = new DashboardService(new FakeEventService(registrations));

        var result = await service.GetUserDashboardAsync("user-1", "User One");

        Assert.Equal("User One", result.UserName);
        Assert.Equal(2, result.RegistrationsCount);
        Assert.Equal(2, result.UpcomingRegistrations.Count);
    }

    private sealed class FakeEventService : IEventService
    {
        private readonly IReadOnlyCollection<RegistrationSummaryViewModel> _registrations;

        public FakeEventService(IReadOnlyCollection<RegistrationSummaryViewModel> registrations)
        {
            _registrations = registrations;
        }

        public Task<IReadOnlyCollection<RegistrationSummaryViewModel>> GetUserRegistrationsAsync(string userId) => Task.FromResult(_registrations);
        public Task<IReadOnlyCollection<ReviewableEventViewModel>> GetReviewableEventsAsync(string userId) => throw new NotImplementedException();
        public Task<ASP_PROJECT.Models.ViewModels.EventListViewModel> GetPublishedEventsAsync(string? searchTerm, int? categoryId, string? statusFilter, int pageNumber, int pageSize) => throw new NotImplementedException();
        public Task<ASP_PROJECT.Models.ViewModels.EventDetailsViewModel?> GetDetailsAsync(int id) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<ASP_PROJECT.Models.Category>> GetCategoriesAsync() => throw new NotImplementedException();
        public Task<IReadOnlyCollection<ASP_PROJECT.Models.Venue>> GetVenuesAsync() => throw new NotImplementedException();
        public Task<ASP_PROJECT.Models.ViewModels.EventEditViewModel> BuildEditorAsync(int? id) => throw new NotImplementedException();
        public Task<int> CreateAsync(ASP_PROJECT.Models.ViewModels.EventEditViewModel model) => throw new NotImplementedException();
        public Task<bool> UpdateAsync(ASP_PROJECT.Models.ViewModels.EventEditViewModel model) => throw new NotImplementedException();
        public Task<bool> RegisterAsync(string userId, ASP_PROJECT.Models.ViewModels.RegistrationInputModel model) => throw new NotImplementedException();
        public Task<bool> RequestRefundAsync(string userId, int registrationId) => throw new NotImplementedException();
        public Task<bool> AddReviewAsync(string userId, ASP_PROJECT.Models.ViewModels.ReviewInputModel model) => throw new NotImplementedException();
        public Task<ASP_PROJECT.Models.ViewModels.AdminDashboardViewModel> GetAdminDashboardAsync() => throw new NotImplementedException();
    }
}
