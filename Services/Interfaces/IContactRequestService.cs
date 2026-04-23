using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IContactRequestService
{
    Task CreateAsync(ContactFormViewModel model, string? actorId = null, string? actorName = null);
    Task<IReadOnlyCollection<ContactRequestManagementItemViewModel>> GetManagementAsync(string? statusFilter = null, string? searchTerm = null, string? sortBy = null);
    Task<ContactRequestUpdateViewModel?> BuildUpdateModelAsync(int id);
    Task<bool> UpdateAsync(ContactRequestUpdateViewModel model, string? actorId, string actorName);
    Task<int> GetOpenCountAsync();
}
