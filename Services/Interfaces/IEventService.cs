using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IEventService
{
    Task<EventListViewModel> GetPublishedEventsAsync(string? searchTerm, int? categoryId, string? statusFilter, int pageNumber, int pageSize);
    Task<EventDetailsViewModel?> GetDetailsAsync(int id);
    Task<IReadOnlyCollection<Category>> GetCategoriesAsync();
    Task<IReadOnlyCollection<Venue>> GetVenuesAsync();
    Task<EventEditViewModel> BuildEditorAsync(int? id);
    Task<int> CreateAsync(EventEditViewModel model);
    Task<bool> UpdateAsync(EventEditViewModel model);
    Task<bool> RegisterAsync(string userId, RegistrationInputModel model);
    Task<bool> RequestRefundAsync(string userId, int registrationId);
    Task<bool> AddReviewAsync(string userId, ReviewInputModel model);
    Task<IReadOnlyCollection<RegistrationSummaryViewModel>> GetUserRegistrationsAsync(string userId);
    Task<IReadOnlyCollection<ReviewableEventViewModel>> GetReviewableEventsAsync(string userId);
    Task<AdminDashboardViewModel> GetAdminDashboardAsync();
}
