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

    public async Task<IReadOnlyCollection<AnnouncementManagementItemViewModel>> GetManagementAsync()
    {
        return await _dbContext.Announcements
            .AsNoTracking()
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.PublishedOnUtc)
            .Select(x => new AnnouncementManagementItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Audience = string.IsNullOrWhiteSpace(x.Audience) ? "All" : x.Audience,
                IsPinned = x.IsPinned,
                PublishedOnUtc = x.PublishedOnUtc,
                ContentPreview = x.Content.Length <= 140 ? x.Content : $"{x.Content.Substring(0, 140)}..."
            })
            .ToListAsync();
    }

    public async Task<AnnouncementEditViewModel> BuildEditorAsync(int? id = null)
    {
        if (!id.HasValue)
        {
            return new AnnouncementEditViewModel
            {
                PublishedOnUtc = DateTime.UtcNow,
                Audience = "All"
            };
        }

        var model = await _dbContext.Announcements
            .AsNoTracking()
            .Where(x => x.Id == id.Value)
            .Select(x => new AnnouncementEditViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                Audience = x.Audience,
                PublishedOnUtc = x.PublishedOnUtc,
                IsPinned = x.IsPinned
            })
            .SingleOrDefaultAsync();

        return model ?? new AnnouncementEditViewModel();
    }

    public async Task<int> CreateAsync(AnnouncementEditViewModel model, string? actorId, string actorName)
    {
        var announcement = new Announcement
        {
            Title = model.Title.Trim(),
            Content = model.Content.Trim(),
            Audience = string.IsNullOrWhiteSpace(model.Audience) ? "All" : model.Audience.Trim(),
            PublishedOnUtc = DateTime.SpecifyKind(model.PublishedOnUtc, DateTimeKind.Utc),
            IsPinned = model.IsPinned
        };

        _dbContext.Announcements.Add(announcement);
        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("Announcement", "Create", actorId, actorName, $"Created announcement {announcement.Title}.", announcement.Id);
        return announcement.Id;
    }

    public async Task<bool> UpdateAsync(AnnouncementEditViewModel model, string? actorId, string actorName)
    {
        if (!model.Id.HasValue)
        {
            return false;
        }

        var announcement = await _dbContext.Announcements.FindAsync(model.Id.Value);
        if (announcement is null)
        {
            return false;
        }

        announcement.Title = model.Title.Trim();
        announcement.Content = model.Content.Trim();
        announcement.Audience = string.IsNullOrWhiteSpace(model.Audience) ? "All" : model.Audience.Trim();
        announcement.PublishedOnUtc = DateTime.SpecifyKind(model.PublishedOnUtc, DateTimeKind.Utc);
        announcement.IsPinned = model.IsPinned;

        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("Announcement", "Update", actorId, actorName, $"Updated announcement {announcement.Title}.", announcement.Id);
        return true;
    }

    private async Task LogAuditAsync(string entityType, string actionType, string? actorId, string actorName, string summary, int? entityId)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            EntityType = entityType,
            ActionType = actionType,
            EntityId = entityId,
            PerformedByUserId = actorId,
            PerformedByName = string.IsNullOrWhiteSpace(actorName) ? "System" : actorName,
            Summary = summary
        });

        await _dbContext.SaveChangesAsync();
    }
}
