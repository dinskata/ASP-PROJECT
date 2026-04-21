using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IVenueService
{
    Task<PagedResult<VenueListItemViewModel>> GetPagedAsync(string? searchTerm, int pageNumber, int pageSize);
    Task<VenueDetailsViewModel?> GetDetailsAsync(int id);
    Task<IReadOnlyCollection<VenueListItemViewModel>> GetAllForManagementAsync(IReadOnlyCollection<int>? allowedVenueIds = null);
    Task<VenueEditViewModel?> BuildEditorAsync(int id, IReadOnlyCollection<int>? allowedVenueIds = null);
    Task<int> CreateAsync(VenueEditViewModel model, string? actorId = null, string? actorName = null);
    Task<bool> UpdateAsync(VenueEditViewModel model, string? actorId = null, string? actorName = null, IReadOnlyCollection<int>? allowedVenueIds = null);
}
