using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;

namespace ASP_PROJECT.Services.Interfaces;

public interface IAnnouncementService
{
    Task<AnnouncementListViewModel> GetAllAsync();
    Task<Announcement?> GetByIdAsync(int id);
}
