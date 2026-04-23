using ASP_PROJECT.Data;
using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Services;

public class ContactRequestService : IContactRequestService
{
    private readonly ApplicationDbContext _dbContext;

    public ContactRequestService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(ContactFormViewModel model, string? actorId = null, string? actorName = null)
    {
        var request = new ContactRequest
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Subject = model.Subject.Trim(),
            Message = model.Message.Trim()
        };

        _dbContext.ContactRequests.Add(request);
        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("ContactRequest", "Create", actorId, actorName, $"Created contact request from {request.FullName}: {request.Subject}.", request.Id);
    }

    public async Task<IReadOnlyCollection<ContactRequestManagementItemViewModel>> GetManagementAsync(string? statusFilter = null, string? searchTerm = null, string? sortBy = null)
    {
        var query = _dbContext.ContactRequests
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) && !string.Equals(statusFilter, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(normalized) ||
                x.Email.ToLower().Contains(normalized) ||
                x.Subject.ToLower().Contains(normalized) ||
                x.Message.ToLower().Contains(normalized));
        }

        query = (sortBy ?? "newest").ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(x => x.CreatedOnUtc),
            "status" => query.OrderBy(x => x.Status).ThenByDescending(x => x.CreatedOnUtc),
            "name" => query.OrderBy(x => x.FullName).ThenByDescending(x => x.CreatedOnUtc),
            _ => query.OrderBy(x => x.Status == ContactRequestStatuses.Open ? 0 : x.Status == ContactRequestStatuses.InProgress ? 1 : 2)
                .ThenByDescending(x => x.CreatedOnUtc)
        };

        return await query
            .Select(x => new ContactRequestManagementItemViewModel
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Subject = x.Subject,
                Status = x.Status,
                CreatedOnUtc = x.CreatedOnUtc,
                MessagePreview = x.Message.Length <= 140 ? x.Message : $"{x.Message.Substring(0, 140)}..."
            })
            .ToListAsync();
    }

    public async Task<ContactRequestUpdateViewModel?> BuildUpdateModelAsync(int id)
    {
        return await _dbContext.ContactRequests
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ContactRequestUpdateViewModel
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Subject = x.Subject,
                Message = x.Message,
                CreatedOnUtc = x.CreatedOnUtc,
                ResolvedOnUtc = x.ResolvedOnUtc,
                ResolvedByName = x.ResolvedByName,
                Status = x.Status,
                InternalNote = x.InternalNote
            })
            .SingleOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(ContactRequestUpdateViewModel model, string? actorId, string actorName)
    {
        var request = await _dbContext.ContactRequests.FindAsync(model.Id);
        if (request is null)
        {
            return false;
        }

        request.Status = model.Status;
        request.InternalNote = model.InternalNote.Trim();

        if (string.Equals(model.Status, ContactRequestStatuses.Resolved, StringComparison.OrdinalIgnoreCase))
        {
            request.ResolvedOnUtc = DateTime.UtcNow;
            request.ResolvedByName = actorName;
        }
        else
        {
            request.ResolvedOnUtc = null;
            request.ResolvedByName = string.Empty;
        }

        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("ContactRequest", "Update", actorId, actorName, $"Updated contact request {request.Subject} to {request.Status}.", request.Id);
        return true;
    }

    public Task<int> GetOpenCountAsync()
        => _dbContext.ContactRequests.CountAsync(x => x.Status == ContactRequestStatuses.Open);

    private async Task LogAuditAsync(string entityType, string actionType, string? actorId, string? actorName, string summary, int? entityId)
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
