using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IManagementService
{
    Task<ManagerDashboardViewModel> GetManagerDashboardAsync(string userId);
    Task<ModeratorDashboardViewModel> GetModeratorDashboardAsync();
    Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetUsersAsync();
    Task<UserAdminEditViewModel?> BuildUserEditorAsync(string userId);
    Task<bool> UpdateUserAsync(UserAdminEditViewModel model, string actorId, string actorName);
    Task<IReadOnlyCollection<VenueListItemViewModel>> GetAssignedVenuesAsync(string userId);
    Task<IReadOnlyCollection<int>> GetAssignedVenueIdsAsync(string userId);
    Task<IReadOnlyCollection<ReviewModerationListItemViewModel>> GetReviewsAsync(string? statusFilter);
    Task<bool> ModerateReviewAsync(int reviewId, bool approve, string actorId, string actorName);
    Task<IReadOnlyCollection<PaymentManagementItemViewModel>> GetPaymentsAsync();
    Task<bool> ForceRefundAsync(int registrationId, string actorId, string actorName);
    Task<IReadOnlyCollection<AuditLogListItemViewModel>> GetAuditLogsAsync(int take = 100);
    Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetVenueManagersAsync();
    Task<VenueAssignmentEditViewModel?> BuildVenueAssignmentsAsync(string userId);
    Task<bool> UpdateVenueAssignmentsAsync(VenueAssignmentEditViewModel model, string actorId, string actorName);
}
