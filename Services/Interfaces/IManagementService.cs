using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IManagementService
{
    Task<ManagerDashboardViewModel> GetManagerDashboardAsync(string userId);
    Task<ModeratorDashboardViewModel> GetModeratorDashboardAsync();
    Task<VenueStatisticsPageViewModel> GetVenueStatisticsAsync(IReadOnlyCollection<int>? allowedVenueIds = null, int? selectedVenueId = null);
    Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetUsersAsync(string? searchTerm = null, string? sortBy = null);
    Task<UserAdminEditViewModel?> BuildUserEditorAsync(string userId);
    Task<bool> UpdateUserAsync(UserAdminEditViewModel model, string actorId, string actorName);
    Task<IReadOnlyCollection<VenueListItemViewModel>> GetAssignedVenuesAsync(string userId);
    Task<IReadOnlyCollection<int>> GetAssignedVenueIdsAsync(string userId);
    Task<IReadOnlyCollection<ReviewModerationListItemViewModel>> GetReviewsAsync(string? statusFilter, string? searchTerm = null, string? sortBy = null);
    Task<bool> ModerateReviewAsync(int reviewId, bool approve, string actorId, string actorName);
    Task<IReadOnlyCollection<PaymentManagementItemViewModel>> GetPaymentsAsync(string? searchTerm = null, string? statusFilter = null, string? sortBy = null);
    Task<bool> ForceRefundAsync(int registrationId, string actorId, string actorName);
    Task<IReadOnlyCollection<AuditLogListItemViewModel>> GetAuditLogsAsync(string? searchTerm = null, string? entityFilter = null, string? sortBy = null, int take = 100);
    Task<IReadOnlyCollection<TicketRegistryItemViewModel>> GetTicketRegistryAsync(string? searchTerm = null, string? statusFilter = null, string? sortBy = null, int take = 250, IReadOnlyCollection<int>? allowedVenueIds = null);
    Task<TicketVerificationResultViewModel?> VerifyTicketAsync(string? ticketCode, string? verificationCode, IReadOnlyCollection<int>? allowedVenueIds = null, bool hasGlobalAccess = false);
    Task<TicketVerificationResultViewModel?> MarkTicketCheckedInAsync(string? ticketCode, IReadOnlyCollection<int>? allowedVenueIds, bool hasGlobalAccess, string? actorId, string actorName);
    Task<AdminUserDetailsViewModel?> GetUserDetailsAsync(string userId);
    Task<AdminRegistrationTicketsViewModel?> GetRegistrationTicketsAsync(int registrationId);
    Task<AdminTicketEditViewModel?> BuildTicketEditorAsync(int ticketId);
    Task<bool> UncheckTicketAsync(int ticketId, string actorId, string actorName);
    Task<bool> UpdateTicketAsync(AdminTicketEditViewModel model, string actorId, string actorName);
    Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetVenueManagersAsync();
    Task<VenueAssignmentEditViewModel?> BuildVenueAssignmentsAsync(string userId);
    Task<bool> UpdateVenueAssignmentsAsync(VenueAssignmentEditViewModel model, string actorId, string actorName);
    Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetVenueStaffAsync(string managerUserId);
    Task<VenueAssignmentEditViewModel?> BuildVenueStaffAssignmentsAsync(string staffUserId, string managerUserId);
    Task<bool> UpdateVenueStaffAssignmentsAsync(VenueAssignmentEditViewModel model, string managerUserId, string actorId, string actorName);
    Task<VenueStaffCreateViewModel> BuildVenueStaffCreateAsync(string managerUserId, VenueStaffCreateViewModel? source = null);
    Task<(bool Succeeded, string? ErrorMessage, string? UserId)> CreateVenueStaffAsync(VenueStaffCreateViewModel model, string managerUserId, string actorId, string actorName);
}
