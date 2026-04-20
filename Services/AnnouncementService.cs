using ASP_PROJECT.Data;
using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _dbContext;

    public AnnouncementService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AnnouncementListViewModel> GetAllAsync()
    {
        var all = await _dbContext.Announcements
            .AsNoTracking()
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.PublishedOnUtc)
            .ToListAsync();

        return new AnnouncementListViewModel
        {
            Pinned = all.Where(x => x.IsPinned).ToList(),
            Recent = all.Where(x => !x.IsPinned).ToList()
        };
    }

    public Task<Announcement?> GetByIdAsync(int id)
        => _dbContext.Announcements.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
}
