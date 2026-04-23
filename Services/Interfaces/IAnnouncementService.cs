using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IAnnouncementService
{
    Task<AnnouncementListViewModel> GetAllAsync();
    Task<Announcement?> GetByIdAsync(int id);
    Task<IReadOnlyCollection<AnnouncementManagementItemViewModel>> GetManagementAsync();
    Task<AnnouncementEditViewModel> BuildEditorAsync(int? id = null);
    Task<int> CreateAsync(AnnouncementEditViewModel model, string? actorId, string actorName);
    Task<bool> UpdateAsync(AnnouncementEditViewModel model, string? actorId, string actorName);
}
