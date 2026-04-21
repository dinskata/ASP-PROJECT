using ASP_PROJECT.Data;
using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Services;

public class VenueService : IVenueService
{
    private readonly ApplicationDbContext _dbContext;

    public VenueService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<VenueListItemViewModel>> GetPagedAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        var query = _dbContext.Venues
            .AsNoTracking()
            .Include(x => x.Events)
            .OrderBy(x => x.City)
            .ThenBy(x => x.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(normalized) || x.City.ToLower().Contains(normalized));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new VenueListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                City = x.City,
                Address = x.Address,
                Capacity = x.Capacity,
                EventCount = x.Events.Count,
                AverageRating = x.Events.SelectMany(e => e.Reviews).Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved) == 0
                    ? 0
                    : x.Events.SelectMany(e => e.Reviews).Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved).Average(r => r.Rating),
                ReviewCount = x.Events.SelectMany(e => e.Reviews).Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved)
            })
            .ToListAsync();

        return new PagedResult<VenueListItemViewModel>
        {
            Items = items,
            PageNumber = pageNumber,
            TotalItems = totalItems,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize))
        };
    }

    public async Task<VenueDetailsViewModel?> GetDetailsAsync(int id)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.Venues
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new VenueDetailsViewModel
            {
                Id = x.Id,
                Name = x.Name,
                City = x.City,
                Address = x.Address,
                Capacity = x.Capacity,
                Description = x.Description,
                AverageRating = x.Events.SelectMany(e => e.Reviews).Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved) == 0
                    ? 0
                    : x.Events.SelectMany(e => e.Reviews).Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved).Average(r => r.Rating),
                ReviewCount = x.Events.SelectMany(e => e.Reviews).Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved),
                UpcomingEvents = x.Events
                    .Where(e => e.IsPublished && e.StartsAtUtc.AddMinutes(e.DurationMinutes) > now)
                    .OrderBy(e => e.StartsAtUtc)
                    .Select(e => new EventListItemViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Category = e.Category!.Name,
                        Venue = x.Name,
                        City = x.City,
                        StartsAtUtc = e.StartsAtUtc,
                        Price = e.Price,
                        SeatsAvailable = e.SeatsAvailable,
                        HasEnded = false,
                        AverageRating = e.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved) == 0
                            ? 0
                            : e.Reviews.Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved).Average(r => r.Rating),
                        ReviewCount = e.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved),
                        IsPublished = e.IsPublished
                    })
                    .ToList(),
                EndedEvents = x.Events
                    .Where(e => e.IsPublished && e.StartsAtUtc.AddMinutes(e.DurationMinutes) <= now)
                    .OrderByDescending(e => e.StartsAtUtc.AddMinutes(e.DurationMinutes))
                    .Select(e => new EventListItemViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Category = e.Category!.Name,
                        Venue = x.Name,
                        City = x.City,
                        StartsAtUtc = e.StartsAtUtc,
                        Price = e.Price,
                        SeatsAvailable = e.SeatsAvailable,
                        HasEnded = true,
                        AverageRating = e.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved) == 0
                            ? 0
                            : e.Reviews.Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved).Average(r => r.Rating),
                        ReviewCount = e.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved),
                        IsPublished = e.IsPublished
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<VenueListItemViewModel>> GetAllForManagementAsync(IReadOnlyCollection<int>? allowedVenueIds = null, string? searchTerm = null, string? sortBy = null)
    {
        var query = _dbContext.Venues
            .AsNoTracking()
            .Include(x => x.Events)
            .AsQueryable();

        if (allowedVenueIds is { Count: > 0 })
        {
            query = query.Where(x => allowedVenueIds.Contains(x.Id));
        }
        else if (allowedVenueIds is not null)
        {
            query = query.Where(_ => false);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(normalized) || x.City.ToLower().Contains(normalized));
        }

        query = (sortBy ?? "city").ToLowerInvariant() switch
        {
            "city_desc" => query.OrderByDescending(x => x.City).ThenBy(x => x.Name),
            "name" => query.OrderBy(x => x.Name),
            "name_desc" => query.OrderByDescending(x => x.Name),
            "capacity_desc" => query.OrderByDescending(x => x.Capacity).ThenBy(x => x.Name),
            "capacity_asc" => query.OrderBy(x => x.Capacity).ThenBy(x => x.Name),
            "events_desc" => query.OrderByDescending(x => x.Events.Count).ThenBy(x => x.Name),
            "events_asc" => query.OrderBy(x => x.Events.Count).ThenBy(x => x.Name),
            _ => query.OrderBy(x => x.City).ThenBy(x => x.Name)
        };

        return await query
            .Select(x => new VenueListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                City = x.City,
                Address = x.Address,
                Capacity = x.Capacity,
                EventCount = x.Events.Count,
                AverageRating = x.Events.SelectMany(e => e.Reviews).Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved) == 0
                    ? 0
                    : x.Events.SelectMany(e => e.Reviews).Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved).Average(r => r.Rating),
                ReviewCount = x.Events.SelectMany(e => e.Reviews).Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved)
            })
            .ToListAsync();
    }

    public async Task<VenueEditViewModel?> BuildEditorAsync(int id, IReadOnlyCollection<int>? allowedVenueIds = null)
    {
        if (allowedVenueIds is not null && !allowedVenueIds.Contains(id))
        {
            return null;
        }

        return await _dbContext.Venues
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new VenueEditViewModel
            {
                Id = x.Id,
                Name = x.Name,
                City = x.City,
                Address = x.Address,
                Capacity = x.Capacity,
                Description = x.Description
            })
            .SingleOrDefaultAsync();
    }

    public async Task<int> CreateAsync(VenueEditViewModel model, string? actorId = null, string? actorName = null)
    {
        var venue = new Venue
        {
            Name = model.Name.Trim(),
            City = model.City.Trim(),
            Address = model.Address.Trim(),
            Capacity = model.Capacity,
            Description = model.Description.Trim()
        };

        _dbContext.Venues.Add(venue);
        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("Venue", "Create", actorId, actorName, $"Created venue {venue.Name}.", venue.Id);
        return venue.Id;
    }

    public async Task<bool> UpdateAsync(VenueEditViewModel model, string? actorId = null, string? actorName = null, IReadOnlyCollection<int>? allowedVenueIds = null)
    {
        if (allowedVenueIds is not null && !allowedVenueIds.Contains(model.Id))
        {
            return false;
        }

        var venue = await _dbContext.Venues.FindAsync(model.Id);
        if (venue is null)
        {
            return false;
        }

        venue.Name = model.Name.Trim();
        venue.City = model.City.Trim();
        venue.Address = model.Address.Trim();
        venue.Capacity = model.Capacity;
        venue.Description = model.Description.Trim();

        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("Venue", "Update", actorId, actorName, $"Updated venue {venue.Name}.", venue.Id);
        return true;
    }

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
