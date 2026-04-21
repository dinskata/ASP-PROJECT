using ASP_PROJECT.Data;
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
            var normalized = searchTerm.Trim().ToLower();
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
                EventCount = x.Events.Count
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
                        AverageRating = e.Reviews.Count == 0 ? 0 : e.Reviews.Average(r => r.Rating),
                        ReviewCount = e.Reviews.Count
                    })
                    .ToList(),
                EndedEvents = x.Events
                    .Where(e => e.IsPublished && e.StartsAtUtc.AddMinutes(e.DurationMinutes) <= now)
                    .OrderByDescending(e => e.StartsAtUtc)
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
                        AverageRating = e.Reviews.Count == 0 ? 0 : e.Reviews.Average(r => r.Rating),
                        ReviewCount = e.Reviews.Count
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<VenueListItemViewModel>> GetAllForManagementAsync()
    {
        return await _dbContext.Venues
            .AsNoTracking()
            .Include(x => x.Events)
            .OrderBy(x => x.City)
            .ThenBy(x => x.Name)
            .Select(x => new VenueListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                City = x.City,
                Address = x.Address,
                Capacity = x.Capacity,
                EventCount = x.Events.Count
            })
            .ToListAsync();
    }

    public async Task<VenueEditViewModel?> BuildEditorAsync(int id)
    {
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

    public async Task<bool> UpdateAsync(VenueEditViewModel model)
    {
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
        return true;
    }
}
