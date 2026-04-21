using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IVenueService
{
    Task<PagedResult<VenueListItemViewModel>> GetPagedAsync(string? searchTerm, int pageNumber, int pageSize);
    Task<VenueDetailsViewModel?> GetDetailsAsync(int id);
    Task<IReadOnlyCollection<VenueListItemViewModel>> GetAllForManagementAsync();
    Task<VenueEditViewModel?> BuildEditorAsync(int id);
    Task<bool> UpdateAsync(VenueEditViewModel model);
}
