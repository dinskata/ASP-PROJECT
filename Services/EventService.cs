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
            var normalized = searchTerm.Trim().ToLowerInvariant();
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
            .ThenBy(x => x.StartsAtUtc.AddMinutes(x.DurationMinutes) <= now
                ? DateTime.MaxValue
                : x.StartsAtUtc)
            .ThenByDescending(x => x.StartsAtUtc.AddMinutes(x.DurationMinutes) <= now
                ? x.StartsAtUtc.AddMinutes(x.DurationMinutes)
                : DateTime.MinValue);

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
                AverageRating = x.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved) == 0
                    ? 0
                    : x.Reviews.Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved).Average(r => r.Rating),
                ReviewCount = x.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved),
                IsPublished = x.IsPublished
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

    public async Task<IReadOnlyCollection<ManagementEventListItemViewModel>> GetManagementEventsAsync(string? searchTerm, IReadOnlyCollection<int>? allowedVenueIds = null)
    {
        var query = _dbContext.Events
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Venue)
            .Include(x => x.Registrations)
            .Include(x => x.Reviews)
            .AsQueryable();

        if (allowedVenueIds is { Count: > 0 })
        {
            query = query.Where(x => allowedVenueIds.Contains(x.VenueId));
        }
        else if (allowedVenueIds is not null)
        {
            query = query.Where(_ => false);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Title.ToLower().Contains(normalized) ||
                x.Category!.Name.ToLower().Contains(normalized) ||
                x.Venue!.Name.ToLower().Contains(normalized));
        }

        return await query
            .OrderBy(x => x.StartsAtUtc)
            .Select(x => new ManagementEventListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Category = x.Category!.Name,
                Venue = x.Venue!.Name,
                StartsAtUtc = x.StartsAtUtc,
                IsPublished = x.IsPublished,
                TicketsSold = x.Registrations.Sum(r => r.Tickets),
                ReviewCount = x.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved)
            })
            .ToListAsync();
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
                AverageRating = x.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved) == 0
                    ? 0
                    : x.Reviews.Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved).Average(r => r.Rating),
                ReviewCount = x.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved),
                Reviews = x.Reviews
                    .Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved)
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

    public async Task<EventEditViewModel> BuildEditorAsync(int? id, IReadOnlyCollection<int>? allowedVenueIds = null)
    {
        var venuesQuery = _dbContext.Venues.AsNoTracking().OrderBy(x => x.Name).AsQueryable();
        if (allowedVenueIds is { Count: > 0 })
        {
            venuesQuery = venuesQuery.Where(x => allowedVenueIds.Contains(x.Id));
        }
        else if (allowedVenueIds is not null)
        {
            venuesQuery = venuesQuery.Where(_ => false);
        }

        var model = new EventEditViewModel
        {
            Categories = await GetCategoriesAsync(),
            Venues = await venuesQuery.ToListAsync()
        };

        if (!id.HasValue)
        {
            return model;
        }

        var entity = await _dbContext.Events.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id.Value);
        if (entity is null)
        {
            return model;
        }

        if (allowedVenueIds is not null && !allowedVenueIds.Contains(entity.VenueId))
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

    public async Task<int> CreateAsync(EventEditViewModel model, string? actorId = null, string? actorName = null, IReadOnlyCollection<int>? allowedVenueIds = null)
    {
        if (allowedVenueIds is not null && !allowedVenueIds.Contains(model.VenueId))
        {
            return 0;
        }

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
        await LogAuditAsync("Event", "Create", actorId, actorName, $"Created event {entity.Title}.", entity.Id);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(EventEditViewModel model, string? actorId = null, string? actorName = null, IReadOnlyCollection<int>? allowedVenueIds = null)
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

        if (allowedVenueIds is not null && (!allowedVenueIds.Contains(entity.VenueId) || !allowedVenueIds.Contains(model.VenueId)))
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
        await LogAuditAsync("Event", "Update", actorId, actorName, $"Updated event {entity.Title}.", entity.Id);
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

        var existingRegistration = await _dbContext.Registrations
            .SingleOrDefaultAsync(x => x.EventId == model.EventId && x.UserId == userId);

        if (existingRegistration is not null && existingRegistration.PaymentStatus != "Refunded")
        {
            return false;
        }

        if (existingRegistration is null)
        {
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
        }
        else
        {
            existingRegistration.Tickets = model.Tickets;
            existingRegistration.CardholderName = model.CardholderName.Trim();
            existingRegistration.CardLast4 = model.CardNumber[^4..];
            existingRegistration.PaymentStatus = "Paid";
            existingRegistration.AmountPaid = model.Tickets * eventEntity.Price;
            existingRegistration.RegisteredOnUtc = DateTime.UtcNow;
            existingRegistration.RefundedOnUtc = null;
        }

        eventEntity.SeatsAvailable -= model.Tickets;
        await _dbContext.SaveChangesAsync();

        var actorName = await GetUserDisplayNameAsync(userId);
        await LogAuditAsync(
            "Payment",
            existingRegistration is null ? "Purchase" : "Repurchase",
            userId,
            actorName,
            $"{(existingRegistration is null ? "Purchased" : "Repurchased")} {model.Tickets} ticket(s) for {eventEntity.Title}.",
            eventEntity.Id);
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
        var actorName = await GetUserDisplayNameAsync(userId);
        await LogAuditAsync("Payment", "RefundRequest", userId, actorName, $"Refund requested for {registration.Event.Title}.", registration.Id);
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

        var registrationExists = await _dbContext.Registrations.AnyAsync(x =>
            x.EventId == model.EventId &&
            x.UserId == userId &&
            x.PaymentStatus == "Paid");
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
            Comment = model.Comment.Trim(),
            ModerationStatus = ReviewModerationStatuses.Pending
        });

        await _dbContext.SaveChangesAsync();
        var actorName = await GetUserDisplayNameAsync(userId);
        await LogAuditAsync("Review", "Create", userId, actorName, $"Submitted review for {eventEntity.Title}.", model.EventId);
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
            .Where(x => x.PaymentStatus == "Paid")
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
                ExistingRating = x.Event.Reviews.Where(r => r.UserId == userId).Select(r => r.Rating).FirstOrDefault(),
                ModerationStatus = x.Event.Reviews.Where(r => r.UserId == userId).Select(r => r.ModerationStatus).FirstOrDefault() ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
    {
        var venueManagerRoleId = await _dbContext.Roles
            .Where(x => x.Name == DbInitializer.VenueManagerRole)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        var siteModeratorRoleId = await _dbContext.Roles
            .Where(x => x.Name == DbInitializer.SiteModeratorRole)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

        return new AdminDashboardViewModel
        {
            EventsCount = await _dbContext.Events.CountAsync(),
            VenuesCount = await _dbContext.Venues.CountAsync(),
            RegistrationsCount = await _dbContext.Registrations.CountAsync(),
            UsersCount = await _dbContext.Users.CountAsync(),
            PendingReviewsCount = await _dbContext.Reviews.CountAsync(x => x.ModerationStatus == ReviewModerationStatuses.Pending),
            RefundedPaymentsCount = await _dbContext.Registrations.CountAsync(x => x.PaymentStatus == "Refunded"),
            VenueManagersCount = string.IsNullOrWhiteSpace(venueManagerRoleId) ? 0 : await _dbContext.UserRoles.CountAsync(x => x.RoleId == venueManagerRoleId),
            SiteModeratorsCount = string.IsNullOrWhiteSpace(siteModeratorRoleId) ? 0 : await _dbContext.UserRoles.CountAsync(x => x.RoleId == siteModeratorRoleId),
            NextEvents = await _dbContext.Events
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Venue)
                .Include(x => x.Reviews)
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
                    AverageRating = x.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved) == 0
                        ? 0
                        : x.Reviews.Where(r => r.ModerationStatus == ReviewModerationStatuses.Approved).Average(r => r.Rating),
                    ReviewCount = x.Reviews.Count(r => r.ModerationStatus == ReviewModerationStatuses.Approved),
                    IsPublished = x.IsPublished
                })
                .ToListAsync(),
            RecentAuditEntries = await _dbContext.AuditLogs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(8)
                .Select(x => new AuditLogListItemViewModel
                {
                    EntityType = x.EntityType,
                    ActionType = x.ActionType,
                    EntityId = x.EntityId,
                    PerformedByName = x.PerformedByName,
                    Summary = x.Summary,
                    CreatedOnUtc = x.CreatedOnUtc
                })
                .ToListAsync()
        };
    }

    private async Task<string> GetUserDisplayNameAsync(string userId)
        => await _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => x.FullName)
            .SingleOrDefaultAsync() ?? "User";

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
