using ASP_PROJECT.Data;
using ASP_PROJECT.Helpers;
using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Services;

public class ManagementService : IManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManagementService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<ManagerDashboardViewModel> GetManagerDashboardAsync(string userId)
    {
        var assignedVenueIds = await GetAssignedVenueIdsAsync(userId);
        var manager = await _userManager.FindByIdAsync(userId);

        return new ManagerDashboardViewModel
        {
            ManagerName = manager?.FullName ?? "Venue Manager",
            AssignedVenuesCount = assignedVenueIds.Count,
            ManagedEventsCount = await _dbContext.Events.CountAsync(x => assignedVenueIds.Contains(x.VenueId)),
            AssignedVenues = await GetAssignedVenuesAsync(userId),
            ManagedEvents = await _dbContext.Events
                .AsNoTracking()
                .Where(x => assignedVenueIds.Contains(x.VenueId))
                .Include(x => x.Category)
                .Include(x => x.Venue)
                .Include(x => x.Registrations)
                .Include(x => x.Reviews)
                .OrderBy(x => x.StartsAtUtc)
                .Take(10)
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
                .ToListAsync()
        };
    }

    public async Task<ModeratorDashboardViewModel> GetModeratorDashboardAsync()
    {
        var venueManagerRoleId = await _dbContext.Roles
            .Where(x => x.Name == DbInitializer.VenueManagerRole)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

        return new ModeratorDashboardViewModel
        {
            PendingReviewsCount = await _dbContext.Reviews.CountAsync(x => x.ModerationStatus == ReviewModerationStatuses.Pending),
            ApprovedReviewsCount = await _dbContext.Reviews.CountAsync(x => x.ModerationStatus == ReviewModerationStatuses.Approved),
            VenueManagersCount = string.IsNullOrWhiteSpace(venueManagerRoleId)
                ? 0
                : await _dbContext.UserRoles.CountAsync(x => x.RoleId == venueManagerRoleId),
            RecentReviews = await _dbContext.Reviews
                .AsNoTracking()
                .Include(x => x.Event)
                    .ThenInclude(x => x!.Venue)
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(8)
                .Select(x => new ReviewModerationListItemViewModel
                {
                    ReviewId = x.Id,
                    EventTitle = x.Event!.Title,
                    VenueName = x.Event.Venue!.Name,
                    AuthorName = x.User!.FullName,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    ModerationStatus = x.ModerationStatus,
                    CreatedOnUtc = x.CreatedOnUtc
                })
                .ToListAsync()
        };
    }

    public async Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetUsersAsync(string? searchTerm = null, string? sortBy = null)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .ToListAsync();

        var result = new List<UserAdminListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserAdminListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                CreatedOnUtc = user.CreatedOnUtc,
                RolesDisplay = roles.Count == 0 ? "No roles" : string.Join(", ", roles),
                AssignedVenuesCount = await _dbContext.UserVenueAssignments.CountAsync(x => x.UserId == user.Id)
            });
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            result = result
                .Where(x =>
                    x.FullName.ToLowerInvariant().Contains(normalized) ||
                    x.Email.ToLowerInvariant().Contains(normalized) ||
                    x.RolesDisplay.ToLowerInvariant().Contains(normalized))
                .ToList();
        }

        result = (sortBy ?? "name").ToLowerInvariant() switch
        {
            "name_desc" => result.OrderByDescending(x => x.FullName).ToList(),
            "email" => result.OrderBy(x => x.Email).ToList(),
            "email_desc" => result.OrderByDescending(x => x.Email).ToList(),
            "created" => result.OrderBy(x => x.CreatedOnUtc).ToList(),
            "created_desc" => result.OrderByDescending(x => x.CreatedOnUtc).ToList(),
            "venues" => result.OrderBy(x => x.AssignedVenuesCount).ToList(),
            "venues_desc" => result.OrderByDescending(x => x.AssignedVenuesCount).ToList(),
            _ => result.OrderBy(x => x.FullName).ToList()
        };

        return result;
    }

    public async Task<AdminUserDetailsViewModel?> GetUserDetailsAsync(string userId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var now = DateTime.UtcNow;

        var purchases = await _dbContext.Registrations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .OrderByDescending(x => x.RegisteredOnUtc)
            .Select(x => new PaymentManagementItemViewModel
            {
                RegistrationId = x.Id,
                UserId = x.UserId,
                EventId = x.EventId,
                EventTitle = x.Event!.Title,
                BuyerName = user.FullName,
                VenueName = x.Event.Venue!.Name,
                StartsAtUtc = x.Event.StartsAtUtc,
                Tickets = x.Tickets,
                AmountPaid = x.AmountPaid,
                PaymentStatus = x.PaymentStatus,
                CardLast4 = x.CardLast4,
                PrimaryTicketCode = x.RegistrationTickets
                    .OrderBy(t => t.TicketNumber)
                    .Select(t => t.TicketCode)
                    .FirstOrDefault() ?? string.Empty,
                RegisteredOnUtc = x.RegisteredOnUtc,
                RefundedOnUtc = x.RefundedOnUtc,
                CanForceRefund = x.PaymentStatus == "Paid"
            })
            .ToListAsync();

        var tickets = await _dbContext.RegistrationTickets
            .AsNoTracking()
            .Where(x => x.Registration!.UserId == userId)
            .Include(x => x.Registration)
                .ThenInclude(x => x!.Event)
                    .ThenInclude(x => x!.Venue)
            .OrderBy(x => x.Registration!.Event!.StartsAtUtc)
            .ThenBy(x => x.TicketNumber)
            .Select(x => new AdminUserTicketViewModel
            {
                TicketId = x.Id,
                RegistrationId = x.RegistrationId,
                EventTitle = x.Registration!.Event!.Title,
                VenueName = x.Registration.Event.Venue!.Name,
                StartsAtUtc = x.Registration.Event.StartsAtUtc,
                TicketCode = x.TicketCode,
                VerificationCode = x.VerificationCode,
                SeatLabel = x.SeatLabel,
                TicketNote = x.TicketNote,
                PaymentStatus = x.Registration.PaymentStatus,
                CanEdit = x.Registration.PaymentStatus == "Paid"
                    && x.Registration.Event.StartsAtUtc.AddMinutes(x.Registration.Event.DurationMinutes) > now
                    && !x.IsCheckedIn,
                IsCheckedIn = x.IsCheckedIn,
                CheckedInOnUtc = x.CheckedInOnUtc,
                CheckedInByName = x.CheckedInByName
            })
            .ToListAsync();

        return new AdminUserDetailsViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            RolesDisplay = roles.Count == 0 ? "No roles" : string.Join(", ", roles),
            CreatedOnUtc = user.CreatedOnUtc,
            Purchases = purchases,
            ActiveTickets = tickets
        };
    }

    public async Task<UserAdminEditViewModel?> BuildUserEditorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var assignedVenueIds = await _dbContext.UserVenueAssignments
            .Where(x => x.UserId == userId)
            .Select(x => x.VenueId)
            .ToListAsync();

        return new UserAdminEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            IsBuyer = roles.Contains(DbInitializer.BuyerRole),
            IsVenueManager = roles.Contains(DbInitializer.VenueManagerRole),
            IsVenueStaff = roles.Contains(DbInitializer.VenueStaffRole),
            IsSiteModerator = roles.Contains(DbInitializer.SiteModeratorRole),
            IsAdministrator = roles.Contains(DbInitializer.AdministratorRole),
            AvailableVenues = await _dbContext.Venues
                .AsNoTracking()
                .OrderBy(x => x.City)
                .ThenBy(x => x.Name)
                .Select(x => new SelectableVenueViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    City = x.City,
                    IsSelected = assignedVenueIds.Contains(x.Id)
                })
                .ToListAsync()
        };
    }

    public async Task<bool> UpdateUserAsync(UserAdminEditViewModel model, string actorId, string actorName)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null)
        {
            return false;
        }

        var previousRoles = await _userManager.GetRolesAsync(user);

        user.FullName = model.FullName.Trim();
        if (!string.Equals(user.Email, model.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            user.Email = model.Email.Trim();
            user.UserName = model.Email.Trim();
            user.NormalizedEmail = model.Email.Trim().ToUpperInvariant();
            user.NormalizedUserName = model.Email.Trim().ToUpperInvariant();
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return false;
        }

        var desiredRoles = new List<string>();
        if (model.IsBuyer)
        {
            desiredRoles.Add(DbInitializer.BuyerRole);
        }

        if (model.IsVenueManager)
        {
            desiredRoles.Add(DbInitializer.VenueManagerRole);
        }

        if (model.IsVenueStaff)
        {
            desiredRoles.Add(DbInitializer.VenueStaffRole);
        }

        if (model.IsSiteModerator)
        {
            desiredRoles.Add(DbInitializer.SiteModeratorRole);
        }

        if (model.IsAdministrator)
        {
            desiredRoles.Add(DbInitializer.AdministratorRole);
        }

        var rolesToRemove = previousRoles.Except(desiredRoles).ToArray();
        var rolesToAdd = desiredRoles.Except(previousRoles).ToArray();

        if (rolesToRemove.Length > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        if (rolesToAdd.Length > 0)
        {
            await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        if (!model.IsVenueManager && !model.IsVenueStaff)
        {
            var assignments = await _dbContext.UserVenueAssignments.Where(x => x.UserId == user.Id).ToListAsync();
            if (assignments.Count > 0)
            {
                _dbContext.UserVenueAssignments.RemoveRange(assignments);
            }
        }
        else
        {
            await SyncVenueAssignmentsAsync(user.Id, model.AvailableVenues.Where(x => x.IsSelected).Select(x => x.Id).ToArray());
        }

        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("User", "Update", actorId, actorName, $"Updated roles and profile for {user.FullName}.", null);
        return true;
    }

    public async Task<IReadOnlyCollection<VenueListItemViewModel>> GetAssignedVenuesAsync(string userId)
    {
        var assignedVenueIds = await GetAssignedVenueIdsAsync(userId);
        return await _dbContext.Venues
            .AsNoTracking()
            .Where(x => assignedVenueIds.Contains(x.Id))
            .OrderBy(x => x.City)
            .ThenBy(x => x.Name)
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

    public async Task<IReadOnlyCollection<int>> GetAssignedVenueIdsAsync(string userId)
        => await _dbContext.UserVenueAssignments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.VenueId)
            .ToListAsync();

    public async Task<IReadOnlyCollection<ReviewModerationListItemViewModel>> GetReviewsAsync(string? statusFilter, string? searchTerm = null, string? sortBy = null)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter) ? "all" : statusFilter.Trim();
        var query = _dbContext.Reviews
            .AsNoTracking()
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.User)
            .AsQueryable();

        if (!string.Equals(normalizedStatus, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.ModerationStatus == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Event!.Title.ToLower().Contains(normalized) ||
                x.Event.Venue!.Name.ToLower().Contains(normalized) ||
                x.User!.FullName.ToLower().Contains(normalized) ||
                x.Comment.ToLower().Contains(normalized));
        }

        query = (sortBy ?? "newest").ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(x => x.CreatedOnUtc),
            "rating_desc" => query.OrderByDescending(x => x.Rating).ThenByDescending(x => x.CreatedOnUtc),
            "rating_asc" => query.OrderBy(x => x.Rating).ThenByDescending(x => x.CreatedOnUtc),
            "status" => query.OrderBy(x => x.ModerationStatus).ThenByDescending(x => x.CreatedOnUtc),
            _ => query.OrderBy(x => x.ModerationStatus == ReviewModerationStatuses.Pending ? 0 : 1).ThenByDescending(x => x.CreatedOnUtc)
        };

        return await query
            .Select(x => new ReviewModerationListItemViewModel
            {
                ReviewId = x.Id,
                EventTitle = x.Event!.Title,
                VenueName = x.Event.Venue!.Name,
                AuthorName = x.User!.FullName,
                Rating = x.Rating,
                Comment = x.Comment,
                ModerationStatus = x.ModerationStatus,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();
    }

    public async Task<bool> ModerateReviewAsync(int reviewId, bool approve, string actorId, string actorName)
    {
        var review = await _dbContext.Reviews
            .Include(x => x.Event)
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == reviewId);
        if (review is null)
        {
            return false;
        }

        review.ModerationStatus = approve ? ReviewModerationStatuses.Approved : ReviewModerationStatuses.Rejected;
        review.ModeratedOnUtc = DateTime.UtcNow;
        review.ModeratedByUserId = actorId;

        await _dbContext.SaveChangesAsync();
        await LogAuditAsync(
            "Review",
            approve ? "Approve" : "Reject",
            actorId,
            actorName,
            $"{(approve ? "Approved" : "Rejected")} review for {review.Event?.Title} by {review.User?.FullName}.",
            review.Id);
        return true;
    }

    public async Task<IReadOnlyCollection<PaymentManagementItemViewModel>> GetPaymentsAsync(string? searchTerm = null, string? statusFilter = null, string? sortBy = null)
    {
        var query = _dbContext.Registrations
            .AsNoTracking()
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) && !string.Equals(statusFilter, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.PaymentStatus == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Event!.Title.ToLower().Contains(normalized) ||
                x.Event.Venue!.Name.ToLower().Contains(normalized) ||
                x.User!.FullName.ToLower().Contains(normalized) ||
                x.CardLast4.ToLower().Contains(normalized) ||
                x.RegistrationTickets.Any(t =>
                    t.TicketCode.ToLower().Contains(normalized) ||
                    t.VerificationCode.ToLower().Contains(normalized) ||
                    t.SeatLabel.ToLower().Contains(normalized)));
        }

        query = (sortBy ?? "newest").ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(x => x.RegisteredOnUtc),
            "amount_desc" => query.OrderByDescending(x => x.AmountPaid).ThenByDescending(x => x.RegisteredOnUtc),
            "amount_asc" => query.OrderBy(x => x.AmountPaid).ThenByDescending(x => x.RegisteredOnUtc),
            "tickets_desc" => query.OrderByDescending(x => x.Tickets).ThenByDescending(x => x.RegisteredOnUtc),
            "tickets_asc" => query.OrderBy(x => x.Tickets).ThenByDescending(x => x.RegisteredOnUtc),
            "event" => query.OrderBy(x => x.Event!.Title).ThenByDescending(x => x.RegisteredOnUtc),
            _ => query.OrderByDescending(x => x.RegisteredOnUtc)
        };

        return await query.Select(x => new PaymentManagementItemViewModel
            {
                RegistrationId = x.Id,
                UserId = x.UserId,
                EventId = x.EventId,
                EventTitle = x.Event!.Title,
                BuyerName = x.User!.FullName,
                VenueName = x.Event.Venue!.Name,
                StartsAtUtc = x.Event.StartsAtUtc,
                Tickets = x.Tickets,
                AmountPaid = x.AmountPaid,
                PaymentStatus = x.PaymentStatus,
                CardLast4 = x.CardLast4,
                PrimaryTicketCode = x.RegistrationTickets
                    .OrderBy(t => t.TicketNumber)
                    .Select(t => t.TicketCode)
                    .FirstOrDefault() ?? string.Empty,
                RegisteredOnUtc = x.RegisteredOnUtc,
                RefundedOnUtc = x.RefundedOnUtc,
                CanForceRefund = x.PaymentStatus == "Paid"
            })
            .ToListAsync();
    }

    public async Task<bool> ForceRefundAsync(int registrationId, string actorId, string actorName)
    {
        var registration = await _dbContext.Registrations
            .Include(x => x.Event)
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == registrationId);
        if (registration is null || registration.Event is null || registration.PaymentStatus == "Refunded")
        {
            return false;
        }

        registration.PaymentStatus = "Refunded";
        registration.RefundedOnUtc = DateTime.UtcNow;
        registration.Event.SeatsAvailable += registration.Tickets;

        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("Payment", "ForceRefund", actorId, actorName, $"Refunded payment for {registration.User?.FullName} on {registration.Event.Title}.", registration.Id);
        return true;
    }

    public async Task<IReadOnlyCollection<AuditLogListItemViewModel>> GetAuditLogsAsync(string? searchTerm = null, string? entityFilter = null, string? sortBy = null, int take = 100)
    {
        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityFilter) && !string.Equals(entityFilter, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.EntityType == entityFilter);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.EntityType.ToLower().Contains(normalized) ||
                x.ActionType.ToLower().Contains(normalized) ||
                x.PerformedByName.ToLower().Contains(normalized) ||
                x.Summary.ToLower().Contains(normalized));
        }

        query = (sortBy ?? "newest").ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(x => x.CreatedOnUtc),
            "entity" => query.OrderBy(x => x.EntityType).ThenByDescending(x => x.CreatedOnUtc),
            "action" => query.OrderBy(x => x.ActionType).ThenByDescending(x => x.CreatedOnUtc),
            "actor" => query.OrderBy(x => x.PerformedByName).ThenByDescending(x => x.CreatedOnUtc),
            _ => query.OrderByDescending(x => x.CreatedOnUtc)
        };

        return await query.Take(take).Select(x => new AuditLogListItemViewModel
            {
                EntityType = x.EntityType,
                ActionType = x.ActionType,
                EntityId = x.EntityId,
                PerformedByName = x.PerformedByName,
                Summary = x.Summary,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<TicketRegistryItemViewModel>> GetTicketRegistryAsync(string? searchTerm = null, string? statusFilter = null, string? sortBy = null, int take = 250, IReadOnlyCollection<int>? allowedVenueIds = null)
    {
        var query = _dbContext.RegistrationTickets
            .AsNoTracking()
            .Include(x => x.Registration)
                .ThenInclude(x => x!.Event)
                    .ThenInclude(x => x!.Venue)
            .Include(x => x.Registration)
                .ThenInclude(x => x!.User)
            .AsQueryable();

        if (allowedVenueIds is not null)
        {
            if (allowedVenueIds.Count == 0)
            {
                return Array.Empty<TicketRegistryItemViewModel>();
            }

            query = query.Where(x => allowedVenueIds.Contains(x.Registration!.Event!.VenueId));
        }

        var tickets = await query
            .OrderByDescending(x => x.Registration!.RegisteredOnUtc)
            .Take(take)
            .Select(x => new TicketRegistryItemViewModel
            {
                RegistrationId = x.RegistrationId,
                EventId = x.Registration!.EventId,
                TicketCode = x.TicketCode,
                VerificationCode = x.VerificationCode,
                EventTitle = x.Registration.Event!.Title,
                BuyerName = x.Registration.User!.FullName,
                VenueName = x.Registration.Event.Venue!.Name,
                SeatLabel = x.SeatLabel,
                PaymentStatus = x.Registration.PaymentStatus,
                StartsAtUtc = x.Registration.Event.StartsAtUtc,
                RegisteredOnUtc = x.Registration.RegisteredOnUtc,
                IsCheckedIn = x.IsCheckedIn,
                CheckedInOnUtc = x.CheckedInOnUtc,
                CheckedInByName = x.CheckedInByName
            })
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(statusFilter) && !string.Equals(statusFilter, "all", StringComparison.OrdinalIgnoreCase))
        {
            tickets = tickets.Where(x => x.PaymentStatus == statusFilter).ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            tickets = tickets.Where(x =>
                x.TicketCode.ToLowerInvariant().Contains(normalized) ||
                x.VerificationCode.ToLowerInvariant().Contains(normalized) ||
                x.EventTitle.ToLowerInvariant().Contains(normalized) ||
                x.BuyerName.ToLowerInvariant().Contains(normalized) ||
                x.VenueName.ToLowerInvariant().Contains(normalized) ||
                x.SeatLabel.ToLowerInvariant().Contains(normalized) ||
                x.CheckedInByName.ToLowerInvariant().Contains(normalized))
                .ToList();
        }

        tickets = (sortBy ?? "newest").ToLowerInvariant() switch
        {
            "event" => tickets.OrderBy(x => x.EventTitle).ThenByDescending(x => x.RegisteredOnUtc).ToList(),
            "buyer" => tickets.OrderBy(x => x.BuyerName).ThenByDescending(x => x.RegisteredOnUtc).ToList(),
            "code" => tickets.OrderBy(x => x.TicketCode).ToList(),
            "start" => tickets.OrderBy(x => x.StartsAtUtc).ToList(),
            "status" => tickets.OrderBy(x => x.PaymentStatus).ThenByDescending(x => x.RegisteredOnUtc).ToList(),
            "checked_desc" => tickets.OrderByDescending(x => x.IsCheckedIn).ThenByDescending(x => x.CheckedInOnUtc ?? DateTime.MinValue).ToList(),
            "checked_asc" => tickets.OrderBy(x => x.IsCheckedIn).ThenBy(x => x.CheckedInOnUtc ?? DateTime.MaxValue).ToList(),
            _ => tickets.OrderByDescending(x => x.RegisteredOnUtc).ToList()
        };

        return tickets;
    }

    public async Task<TicketVerificationResultViewModel?> VerifyTicketAsync(string? ticketCode, string? verificationCode, IReadOnlyCollection<int>? allowedVenueIds = null, bool hasGlobalAccess = false)
    {
        var normalizedTicketCode = ticketCode?.Trim().ToUpperInvariant() ?? string.Empty;
        var normalizedVerificationCode = verificationCode?.Trim().ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedTicketCode) || string.IsNullOrWhiteSpace(normalizedVerificationCode))
        {
            return new TicketVerificationResultViewModel
            {
                IsSubmitted = true,
                IsValid = false,
                TicketCode = normalizedTicketCode,
                VerificationCode = normalizedVerificationCode,
                Message = "Enter both the ticket code and verification code."
            };
        }

        var ticket = await _dbContext.RegistrationTickets
            .AsNoTracking()
            .Include(x => x.Registration)
                .ThenInclude(x => x!.Event)
                    .ThenInclude(x => x!.Venue)
            .Include(x => x.Registration)
                .ThenInclude(x => x!.User)
            .SingleOrDefaultAsync(x => x.TicketCode == normalizedTicketCode);

        if (ticket is null || ticket.Registration is null || ticket.Registration.Event is null)
        {
            return new TicketVerificationResultViewModel
            {
                IsSubmitted = true,
                IsValid = false,
                TicketCode = normalizedTicketCode,
                VerificationCode = normalizedVerificationCode,
                Message = "Ticket was not found."
            };
        }

        if (!hasGlobalAccess && allowedVenueIds is not null && !allowedVenueIds.Contains(ticket.Registration.Event.VenueId))
        {
            return new TicketVerificationResultViewModel
            {
                IsSubmitted = true,
                IsValid = false,
                TicketCode = normalizedTicketCode,
                VerificationCode = normalizedVerificationCode,
                Message = "You do not have access to verify tickets for this venue."
            };
        }

        var paymentStatus = ticket.Registration.PaymentStatus;
        var hasEnded = ticket.Registration.Event.StartsAtUtc.AddMinutes(ticket.Registration.Event.DurationMinutes) <= DateTime.UtcNow;
        var verificationMatches = string.Equals(ticket.VerificationCode, normalizedVerificationCode, StringComparison.OrdinalIgnoreCase);
        var isValid = string.Equals(ticket.VerificationCode, normalizedVerificationCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(paymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)
            && !hasEnded
            && !ticket.IsCheckedIn;

        return new TicketVerificationResultViewModel
        {
            IsSubmitted = true,
            IsValid = isValid,
            TicketCode = normalizedTicketCode,
            VerificationCode = normalizedVerificationCode,
            EventTitle = ticket.Registration.Event.Title,
            VenueName = ticket.Registration.Event.Venue?.Name ?? "Unknown venue",
            BuyerName = ticket.Registration.User?.FullName ?? "Unknown buyer",
            SeatLabel = ticket.SeatLabel,
            PaymentStatus = paymentStatus,
            StartsAtUtc = ticket.Registration.Event.StartsAtUtc,
            IsCheckedIn = ticket.IsCheckedIn,
            CheckedInOnUtc = ticket.CheckedInOnUtc,
            CheckedInByName = ticket.CheckedInByName,
            CanMarkCheckedIn = verificationMatches
                && string.Equals(paymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)
                && !hasEnded
                && !ticket.IsCheckedIn,
            Message = !verificationMatches
                ? "Verification code does not match this ticket."
                : string.Equals(paymentStatus, "Refunded", StringComparison.OrdinalIgnoreCase)
                    ? "Ticket has been refunded and is no longer valid."
                    : hasEnded
                        ? "Event has already ended, so this ticket is expired."
                        : ticket.IsCheckedIn
                            ? $"Ticket has already been checked in{(string.IsNullOrWhiteSpace(ticket.CheckedInByName) ? string.Empty : $" by {ticket.CheckedInByName}")}."
                            : "Ticket is valid for entry."
        };
    }

    public async Task<TicketVerificationResultViewModel?> MarkTicketCheckedInAsync(string? ticketCode, IReadOnlyCollection<int>? allowedVenueIds, bool hasGlobalAccess, string? actorId, string actorName)
    {
        var normalizedTicketCode = ticketCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedTicketCode))
        {
            return new TicketVerificationResultViewModel
            {
                IsSubmitted = true,
                IsValid = false,
                TicketCode = normalizedTicketCode,
                Message = "Ticket code is required."
            };
        }

        var ticket = await _dbContext.RegistrationTickets
            .Include(x => x.Registration)
                .ThenInclude(x => x!.Event)
                    .ThenInclude(x => x!.Venue)
            .Include(x => x.Registration)
                .ThenInclude(x => x!.User)
            .SingleOrDefaultAsync(x => x.TicketCode == normalizedTicketCode);

        if (ticket is null || ticket.Registration is null || ticket.Registration.Event is null)
        {
            return new TicketVerificationResultViewModel
            {
                IsSubmitted = true,
                IsValid = false,
                TicketCode = normalizedTicketCode,
                Message = "Ticket was not found."
            };
        }

        if (!hasGlobalAccess && allowedVenueIds is not null && !allowedVenueIds.Contains(ticket.Registration.Event.VenueId))
        {
            return new TicketVerificationResultViewModel
            {
                IsSubmitted = true,
                IsValid = false,
                TicketCode = normalizedTicketCode,
                VerificationCode = ticket.VerificationCode,
                Message = "You do not have access to check in tickets for this venue."
            };
        }

        var paymentStatus = ticket.Registration.PaymentStatus;
        var hasEnded = ticket.Registration.Event.StartsAtUtc.AddMinutes(ticket.Registration.Event.DurationMinutes) <= DateTime.UtcNow;
        if (!string.Equals(paymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) || hasEnded || ticket.IsCheckedIn)
        {
            return await VerifyTicketAsync(ticket.TicketCode, ticket.VerificationCode, allowedVenueIds, hasGlobalAccess);
        }

        ticket.IsCheckedIn = true;
        ticket.CheckedInOnUtc = DateTime.UtcNow;
        ticket.CheckedInByName = actorName;

        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("Ticket", "CheckIn", actorId, actorName, $"Checked in ticket {ticket.TicketCode} for {ticket.Registration.Event.Title}.", ticket.Id);

        return await VerifyTicketAsync(ticket.TicketCode, ticket.VerificationCode, allowedVenueIds, hasGlobalAccess);
    }

    public async Task<AdminTicketEditViewModel?> BuildTicketEditorAsync(int ticketId)
    {
        return await _dbContext.RegistrationTickets
            .AsNoTracking()
            .Include(x => x.Registration)
                .ThenInclude(x => x!.Event)
                    .ThenInclude(x => x!.Venue)
            .Include(x => x.Registration)
                .ThenInclude(x => x!.User)
            .Where(x => x.Id == ticketId)
            .Select(x => new AdminTicketEditViewModel
            {
                TicketId = x.Id,
                RegistrationId = x.RegistrationId,
                UserId = x.Registration!.UserId,
                BuyerName = x.Registration.User!.FullName,
                EventTitle = x.Registration.Event!.Title,
                VenueName = x.Registration.Event.Venue!.Name,
                StartsAtUtc = x.Registration.Event.StartsAtUtc,
                TicketCode = x.TicketCode,
                VerificationCode = x.VerificationCode,
                PaymentStatus = x.Registration.PaymentStatus,
                SeatLabel = x.SeatLabel,
                TicketNote = x.TicketNote
            })
            .SingleOrDefaultAsync();
    }

    public async Task<bool> UpdateTicketAsync(AdminTicketEditViewModel model, string actorId, string actorName)
    {
        var ticket = await _dbContext.RegistrationTickets
            .Include(x => x.Registration)
                .ThenInclude(x => x!.Event)
            .SingleOrDefaultAsync(x => x.Id == model.TicketId);
        if (ticket is null || ticket.Registration is null || ticket.Registration.Event is null)
        {
            return false;
        }

        var canEdit = ticket.Registration.PaymentStatus == "Paid"
            && ticket.Registration.Event.StartsAtUtc.AddMinutes(ticket.Registration.Event.DurationMinutes) > DateTime.UtcNow
            && !ticket.IsCheckedIn;
        if (!canEdit)
        {
            return false;
        }

        ticket.SeatLabel = model.SeatLabel.Trim();
        ticket.TicketNote = model.TicketNote.Trim();

        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("Ticket", "Update", actorId, actorName, $"Updated ticket {ticket.TicketCode} for {ticket.Registration.Event.Title}.", ticket.Id);
        return true;
    }

    public async Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetVenueManagersAsync()
    {
        var roleId = await _dbContext.Roles
            .Where(x => x.Name == DbInitializer.VenueManagerRole)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(roleId))
        {
            return Array.Empty<UserAdminListItemViewModel>();
        }

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(x => _dbContext.UserRoles.Any(ur => ur.UserId == x.Id && ur.RoleId == roleId))
            .OrderBy(x => x.FullName)
            .ToListAsync();

        return users.Select(x => new UserAdminListItemViewModel
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email ?? string.Empty,
            CreatedOnUtc = x.CreatedOnUtc,
            RolesDisplay = DbInitializer.VenueManagerRole,
            AssignedVenuesCount = _dbContext.UserVenueAssignments.Count(a => a.UserId == x.Id)
        }).ToList();
    }

    public async Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetVenueStaffAsync(string managerUserId)
    {
        var allowedVenueIds = await GetAssignedVenueIdsAsync(managerUserId);
        if (allowedVenueIds.Count == 0)
        {
            return Array.Empty<UserAdminListItemViewModel>();
        }

        var roleId = await _dbContext.Roles
            .Where(x => x.Name == DbInitializer.VenueStaffRole)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(roleId))
        {
            return Array.Empty<UserAdminListItemViewModel>();
        }

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(x =>
                _dbContext.UserRoles.Any(ur => ur.UserId == x.Id && ur.RoleId == roleId) &&
                _dbContext.UserVenueAssignments.Any(a => a.UserId == x.Id && allowedVenueIds.Contains(a.VenueId)))
            .OrderBy(x => x.FullName)
            .ToListAsync();

        return users.Select(x => new UserAdminListItemViewModel
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email ?? string.Empty,
            CreatedOnUtc = x.CreatedOnUtc,
            RolesDisplay = DbInitializer.VenueStaffRole,
            AssignedVenuesCount = _dbContext.UserVenueAssignments.Count(a => a.UserId == x.Id && allowedVenueIds.Contains(a.VenueId))
        }).ToList();
    }

    public async Task<VenueAssignmentEditViewModel?> BuildVenueAssignmentsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !await _userManager.IsInRoleAsync(user, DbInitializer.VenueManagerRole))
        {
            return null;
        }

        var assignedVenueIds = await GetAssignedVenueIdsAsync(userId);
        return new VenueAssignmentEditViewModel
        {
            UserId = user.Id,
            UserName = user.FullName,
            UserEmail = user.Email ?? string.Empty,
            AvailableVenues = await _dbContext.Venues
                .AsNoTracking()
                .OrderBy(x => x.City)
                .ThenBy(x => x.Name)
                .Select(x => new SelectableVenueViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    City = x.City,
                    IsSelected = assignedVenueIds.Contains(x.Id)
                })
                .ToListAsync()
        };
    }

    public async Task<VenueAssignmentEditViewModel?> BuildVenueStaffAssignmentsAsync(string staffUserId, string managerUserId)
    {
        var user = await _userManager.FindByIdAsync(staffUserId);
        if (user is null || !await _userManager.IsInRoleAsync(user, DbInitializer.VenueStaffRole))
        {
            return null;
        }

        var allowedVenueIds = await GetAssignedVenueIdsAsync(managerUserId);
        if (allowedVenueIds.Count == 0)
        {
            return null;
        }

        var hasManagedAssignment = await _dbContext.UserVenueAssignments
            .AnyAsync(x => x.UserId == staffUserId && allowedVenueIds.Contains(x.VenueId));
        if (!hasManagedAssignment)
        {
            return null;
        }

        var assignedVenueIds = await _dbContext.UserVenueAssignments
            .Where(x => x.UserId == staffUserId && allowedVenueIds.Contains(x.VenueId))
            .Select(x => x.VenueId)
            .ToListAsync();

        return new VenueAssignmentEditViewModel
        {
            UserId = user.Id,
            UserName = user.FullName,
            UserEmail = user.Email ?? string.Empty,
            AvailableVenues = await _dbContext.Venues
                .AsNoTracking()
                .Where(x => allowedVenueIds.Contains(x.Id))
                .OrderBy(x => x.City)
                .ThenBy(x => x.Name)
                .Select(x => new SelectableVenueViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    City = x.City,
                    IsSelected = assignedVenueIds.Contains(x.Id)
                })
                .ToListAsync()
        };
    }

    public async Task<bool> UpdateVenueAssignmentsAsync(VenueAssignmentEditViewModel model, string actorId, string actorName)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null || !await _userManager.IsInRoleAsync(user, DbInitializer.VenueManagerRole))
        {
            return false;
        }

        await SyncVenueAssignmentsAsync(model.UserId, model.AvailableVenues.Where(x => x.IsSelected).Select(x => x.Id).ToArray());
        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("VenueAssignment", "Update", actorId, actorName, $"Updated venue assignments for {user.FullName}.", null);
        return true;
    }

    public async Task<bool> UpdateVenueStaffAssignmentsAsync(VenueAssignmentEditViewModel model, string managerUserId, string actorId, string actorName)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null || !await _userManager.IsInRoleAsync(user, DbInitializer.VenueStaffRole))
        {
            return false;
        }

        var allowedVenueIds = await GetAssignedVenueIdsAsync(managerUserId);
        if (allowedVenueIds.Count == 0)
        {
            return false;
        }

        var hasManagedAssignment = await _dbContext.UserVenueAssignments
            .AnyAsync(x => x.UserId == model.UserId && allowedVenueIds.Contains(x.VenueId));
        if (!hasManagedAssignment)
        {
            return false;
        }

        var selectedVenueIds = model.AvailableVenues
            .Where(x => x.IsSelected && allowedVenueIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToArray();

        await SyncScopedVenueAssignmentsAsync(model.UserId, selectedVenueIds, allowedVenueIds);
        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("VenueStaffAssignment", "Update", actorId, actorName, $"Updated venue staff assignments for {user.FullName}.", null);
        return true;
    }

    public async Task<VenueStaffCreateViewModel> BuildVenueStaffCreateAsync(string managerUserId, VenueStaffCreateViewModel? source = null)
    {
        var allowedVenueIds = await GetAssignedVenueIdsAsync(managerUserId);
        var selectedVenueIds = source?.AvailableVenues.Where(x => x.IsSelected).Select(x => x.Id).ToHashSet() ?? new HashSet<int>();

        return new VenueStaffCreateViewModel
        {
            FullName = source?.FullName ?? string.Empty,
            Email = source?.Email ?? string.Empty,
            Password = source?.Password ?? string.Empty,
            ConfirmPassword = source?.ConfirmPassword ?? string.Empty,
            AvailableVenues = await _dbContext.Venues
                .AsNoTracking()
                .Where(x => allowedVenueIds.Contains(x.Id))
                .OrderBy(x => x.City)
                .ThenBy(x => x.Name)
                .Select(x => new SelectableVenueViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    City = x.City,
                    IsSelected = selectedVenueIds.Contains(x.Id)
                })
                .ToListAsync()
        };
    }

    public async Task<(bool Succeeded, string? ErrorMessage, string? UserId)> CreateVenueStaffAsync(VenueStaffCreateViewModel model, string managerUserId, string actorId, string actorName)
    {
        var allowedVenueIds = await GetAssignedVenueIdsAsync(managerUserId);
        if (allowedVenueIds.Count == 0)
        {
            return (false, "You do not manage any venues yet.", null);
        }

        var selectedVenueIds = model.AvailableVenues
            .Where(x => x.IsSelected && allowedVenueIds.Contains(x.Id))
            .Select(x => x.Id)
            .Distinct()
            .ToArray();

        if (selectedVenueIds.Length == 0)
        {
            return (false, "Select at least one venue for this staff account.", null);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (existingUser is not null)
        {
            return (false, "A user with that email already exists.", null);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            FullName = model.FullName.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            var errorMessage = string.Join(" ", createResult.Errors.Select(x => x.Description));
            return (false, errorMessage, null);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, DbInitializer.VenueStaffRole);
        if (!roleResult.Succeeded)
        {
            var errorMessage = string.Join(" ", roleResult.Errors.Select(x => x.Description));
            return (false, errorMessage, null);
        }

        await SyncScopedVenueAssignmentsAsync(user.Id, selectedVenueIds, allowedVenueIds);
        await _dbContext.SaveChangesAsync();
        await LogAuditAsync("VenueStaff", "Create", actorId, actorName, $"Created venue staff account for {user.FullName}.", null);
        return (true, null, user.Id);
    }

    private async Task SyncVenueAssignmentsAsync(string userId, IReadOnlyCollection<int> selectedVenueIds)
    {
        var existingAssignments = await _dbContext.UserVenueAssignments
            .Where(x => x.UserId == userId)
            .ToListAsync();

        var assignmentsToRemove = existingAssignments.Where(x => !selectedVenueIds.Contains(x.VenueId)).ToList();
        if (assignmentsToRemove.Count > 0)
        {
            _dbContext.UserVenueAssignments.RemoveRange(assignmentsToRemove);
        }

        var existingVenueIds = existingAssignments.Select(x => x.VenueId).ToHashSet();
        var venueIdsToAdd = selectedVenueIds.Where(x => !existingVenueIds.Contains(x)).ToList();
        foreach (var venueId in venueIdsToAdd)
        {
            _dbContext.UserVenueAssignments.Add(new UserVenueAssignment
            {
                UserId = userId,
                VenueId = venueId
            });
        }
    }

    private async Task SyncScopedVenueAssignmentsAsync(string userId, IReadOnlyCollection<int> selectedVenueIds, IReadOnlyCollection<int> allowedVenueIds)
    {
        var scopedAssignments = await _dbContext.UserVenueAssignments
            .Where(x => x.UserId == userId && allowedVenueIds.Contains(x.VenueId))
            .ToListAsync();

        var assignmentsToRemove = scopedAssignments
            .Where(x => !selectedVenueIds.Contains(x.VenueId))
            .ToList();

        if (assignmentsToRemove.Count > 0)
        {
            _dbContext.UserVenueAssignments.RemoveRange(assignmentsToRemove);
        }

        var existingVenueIds = scopedAssignments.Select(x => x.VenueId).ToHashSet();
        var venueIdsToAdd = selectedVenueIds.Where(x => !existingVenueIds.Contains(x)).ToList();
        foreach (var venueId in venueIdsToAdd)
        {
            _dbContext.UserVenueAssignments.Add(new UserVenueAssignment
            {
                UserId = userId,
                VenueId = venueId
            });
        }
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
