using ASP_PROJECT.Data;
using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Services;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _dbContext;

    public EventService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EventListViewModel> GetPublishedEventsAsync(string? searchTerm, int? categoryId, string? statusFilter, int pageNumber, int pageSize)
    {
        var normalizedStatusFilter = string.IsNullOrWhiteSpace(statusFilter)
            ? "all"
            : statusFilter.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        var query = _dbContext.Events
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .Include(x => x.Category)
            .Include(x => x.Venue)
            .Include(x => x.Reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(normalized) ||
                x.Venue!.City.ToLower().Contains(normalized) ||
                x.Category!.Name.ToLower().Contains(normalized));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (normalizedStatusFilter == "upcoming")
        {
            query = query.Where(x => x.StartsAtUtc.AddMinutes(x.DurationMinutes) > now);
        }
        else if (normalizedStatusFilter == "ended")
        {
            query = query.Where(x => x.StartsAtUtc.AddMinutes(x.DurationMinutes) <= now);
        }

        query = query
            .OrderBy(x => x.StartsAtUtc.AddMinutes(x.DurationMinutes) <= now)
            .ThenBy(x => x.StartsAtUtc);

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EventListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Category = x.Category!.Name,
                Venue = x.Venue!.Name,
                City = x.Venue.City,
                StartsAtUtc = x.StartsAtUtc,
                Price = x.Price,
                SeatsAvailable = x.SeatsAvailable,
                HasEnded = x.StartsAtUtc.AddMinutes(x.DurationMinutes) <= now,
                AverageRating = x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating),
                ReviewCount = x.Reviews.Count
            })
            .ToListAsync();

        return new EventListViewModel
        {
            SearchTerm = searchTerm ?? string.Empty,
            CategoryId = categoryId,
            StatusFilter = normalizedStatusFilter,
            Categories = await GetCategoriesAsync(),
            Result = new PagedResult<EventListItemViewModel>
            {
                Items = items,
                PageNumber = pageNumber,
                TotalItems = totalItems,
                TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize))
            }
        };
    }

    public async Task<EventDetailsViewModel?> GetDetailsAsync(int id)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Venue)
            .Include(x => x.Reviews)
                .ThenInclude(x => x.User)
            .Where(x => x.Id == id && x.IsPublished)
            .Select(x => new EventDetailsViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Category = x.Category!.Name,
                Venue = x.Venue!.Name,
                VenueAddress = x.Venue.Address,
                City = x.Venue.City,
                StartsAtUtc = x.StartsAtUtc,
                DurationMinutes = x.DurationMinutes,
                Price = x.Price,
                SeatsAvailable = x.SeatsAvailable,
                HasEnded = x.StartsAtUtc.AddMinutes(x.DurationMinutes) <= DateTime.UtcNow,
                AverageRating = x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating),
                ReviewCount = x.Reviews.Count,
                Reviews = x.Reviews
                    .OrderByDescending(r => r.CreatedOnUtc)
                    .Select(r => new ReviewSummaryViewModel
                    {
                        AuthorName = r.User!.FullName,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedOnUtc = r.CreatedOnUtc
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<Category>> GetCategoriesAsync()
        => await _dbContext.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    public async Task<IReadOnlyCollection<Venue>> GetVenuesAsync()
        => await _dbContext.Venues.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    public async Task<EventEditViewModel> BuildEditorAsync(int? id)
    {
        var model = new EventEditViewModel
        {
            Categories = await GetCategoriesAsync(),
            Venues = await GetVenuesAsync()
        };

        if (!id.HasValue)
        {
            return model;
        }

        var entity = await _dbContext.Events.FindAsync(id.Value);
        if (entity is null)
        {
            return model;
        }

        model.Id = entity.Id;
        model.Title = entity.Title;
        model.Description = entity.Description;
        model.StartsAtUtc = entity.StartsAtUtc;
        model.DurationMinutes = entity.DurationMinutes;
        model.Price = entity.Price;
        model.SeatsAvailable = entity.SeatsAvailable;
        model.IsPublished = entity.IsPublished;
        model.CategoryId = entity.CategoryId;
        model.VenueId = entity.VenueId;
        return model;
    }

    public async Task<int> CreateAsync(EventEditViewModel model)
    {
        var entity = new Event
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            StartsAtUtc = model.StartsAtUtc,
            DurationMinutes = model.DurationMinutes,
            Price = model.Price,
            SeatsAvailable = model.SeatsAvailable,
            IsPublished = model.IsPublished,
            CategoryId = model.CategoryId,
            VenueId = model.VenueId
        };

        _dbContext.Events.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(EventEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return false;
        }

        var entity = await _dbContext.Events.FindAsync(model.Id.Value);
        if (entity is null)
        {
            return false;
        }

        entity.Title = model.Title.Trim();
        entity.Description = model.Description.Trim();
        entity.StartsAtUtc = model.StartsAtUtc;
        entity.DurationMinutes = model.DurationMinutes;
        entity.Price = model.Price;
        entity.SeatsAvailable = model.SeatsAvailable;
        entity.IsPublished = model.IsPublished;
        entity.CategoryId = model.CategoryId;
        entity.VenueId = model.VenueId;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegisterAsync(string userId, RegistrationInputModel model)
    {
        var eventEntity = await _dbContext.Events.SingleOrDefaultAsync(x => x.Id == model.EventId && x.IsPublished);
        if (eventEntity is null || model.Tickets > eventEntity.SeatsAvailable || eventEntity.StartsAtUtc <= DateTime.UtcNow)
        {
            return false;
        }

        if (!string.Equals(model.PaymentDecision, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var alreadyRegistered = await _dbContext.Registrations.AnyAsync(x => x.EventId == model.EventId && x.UserId == userId);
        if (alreadyRegistered)
        {
            return false;
        }

        _dbContext.Registrations.Add(new Registration
        {
            EventId = model.EventId,
            UserId = userId,
            Tickets = model.Tickets,
            CardholderName = model.CardholderName.Trim(),
            CardLast4 = model.CardNumber[^4..],
            PaymentStatus = "Paid",
            AmountPaid = model.Tickets * eventEntity.Price
        });

        eventEntity.SeatsAvailable -= model.Tickets;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RequestRefundAsync(string userId, int registrationId)
    {
        var registration = await _dbContext.Registrations
            .Include(x => x.Event)
            .SingleOrDefaultAsync(x => x.Id == registrationId && x.UserId == userId);
        if (registration is null || registration.Event is null)
        {
            return false;
        }

        if (registration.PaymentStatus == "Refunded")
        {
            return false;
        }

        if (registration.Event.StartsAtUtc <= DateTime.UtcNow.AddHours(48))
        {
            return false;
        }

        registration.PaymentStatus = "Refunded";
        registration.RefundedOnUtc = DateTime.UtcNow;
        registration.Event.SeatsAvailable += registration.Tickets;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddReviewAsync(string userId, ReviewInputModel model)
    {
        var eventEntity = await _dbContext.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == model.EventId && x.IsPublished);
        if (eventEntity is null || eventEntity.StartsAtUtc.AddMinutes(eventEntity.DurationMinutes) > DateTime.UtcNow)
        {
            return false;
        }

        var registrationExists = await _dbContext.Registrations.AnyAsync(x => x.EventId == model.EventId && x.UserId == userId);
        if (!registrationExists)
        {
            return false;
        }

        var existingReview = await _dbContext.Reviews.AnyAsync(x => x.EventId == model.EventId && x.UserId == userId);
        if (existingReview)
        {
            return false;
        }

        _dbContext.Reviews.Add(new Review
        {
            EventId = model.EventId,
            UserId = userId,
            Rating = model.Rating,
            Comment = model.Comment.Trim()
        });

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyCollection<RegistrationSummaryViewModel>> GetUserRegistrationsAsync(string userId)
    {
        return await _dbContext.Registrations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .OrderBy(x => x.Event!.StartsAtUtc)
            .Select(x => new RegistrationSummaryViewModel
            {
                RegistrationId = x.Id,
                EventId = x.EventId,
                EventTitle = x.Event!.Title,
                StartsAtUtc = x.Event.StartsAtUtc,
                Tickets = x.Tickets,
                Venue = x.Event.Venue!.Name,
                AmountPaid = x.AmountPaid,
                PaymentStatus = x.PaymentStatus,
                CardLast4 = x.CardLast4,
                CanRequestRefund = x.PaymentStatus == "Paid" && x.Event.StartsAtUtc > DateTime.UtcNow.AddHours(48)
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<ReviewableEventViewModel>> GetReviewableEventsAsync(string userId)
    {
        return await _dbContext.Registrations
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Event != null && x.Event.IsPublished)
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.Event)
                .ThenInclude(x => x!.Reviews)
            .Where(x => x.Event!.StartsAtUtc.AddMinutes(x.Event.DurationMinutes) <= DateTime.UtcNow)
            .OrderByDescending(x => x.Event!.StartsAtUtc)
            .Select(x => new ReviewableEventViewModel
            {
                EventId = x.EventId,
                EventTitle = x.Event!.Title,
                Venue = x.Event.Venue!.Name,
                StartsAtUtc = x.Event.StartsAtUtc,
                HasExistingReview = x.Event.Reviews.Any(r => r.UserId == userId),
                ExistingComment = x.Event.Reviews.Where(r => r.UserId == userId).Select(r => r.Comment).FirstOrDefault() ?? string.Empty,
                ExistingRating = x.Event.Reviews.Where(r => r.UserId == userId).Select(r => r.Rating).FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
    {
        return new AdminDashboardViewModel
        {
            EventsCount = await _dbContext.Events.CountAsync(),
            VenuesCount = await _dbContext.Venues.CountAsync(),
            RegistrationsCount = await _dbContext.Registrations.CountAsync(),
            UsersCount = await _dbContext.Users.CountAsync(),
            NextEvents = await _dbContext.Events
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Venue)
                .OrderBy(x => x.StartsAtUtc)
                .Take(5)
                .Select(x => new EventListItemViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Category = x.Category!.Name,
                    Venue = x.Venue!.Name,
                City = x.Venue.City,
                StartsAtUtc = x.StartsAtUtc,
                Price = x.Price,
                SeatsAvailable = x.SeatsAvailable,
                HasEnded = x.StartsAtUtc.AddMinutes(x.DurationMinutes) <= DateTime.UtcNow,
                AverageRating = 0,
                ReviewCount = 0
            })
            .ToListAsync()
        };
    }
}
