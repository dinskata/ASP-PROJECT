using ASP_PROJECT.Data;
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

    public async Task<IReadOnlyCollection<UserAdminListItemViewModel>> GetUsersAsync()
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.FullName)
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

        return result;
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

        if (!model.IsVenueManager)
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

    public async Task<IReadOnlyCollection<ReviewModerationListItemViewModel>> GetReviewsAsync(string? statusFilter)
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

        return await query
            .OrderBy(x => x.ModerationStatus == ReviewModerationStatuses.Pending ? 0 : 1)
            .ThenByDescending(x => x.CreatedOnUtc)
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

    public async Task<IReadOnlyCollection<PaymentManagementItemViewModel>> GetPaymentsAsync()
    {
        return await _dbContext.Registrations
            .AsNoTracking()
            .Include(x => x.Event)
                .ThenInclude(x => x!.Venue)
            .Include(x => x.User)
            .OrderByDescending(x => x.RegisteredOnUtc)
            .Select(x => new PaymentManagementItemViewModel
            {
                RegistrationId = x.Id,
                EventId = x.EventId,
                EventTitle = x.Event!.Title,
                BuyerName = x.User!.FullName,
                VenueName = x.Event.Venue!.Name,
                StartsAtUtc = x.Event.StartsAtUtc,
                Tickets = x.Tickets,
                AmountPaid = x.AmountPaid,
                PaymentStatus = x.PaymentStatus,
                CardLast4 = x.CardLast4,
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

    public async Task<IReadOnlyCollection<AuditLogListItemViewModel>> GetAuditLogsAsync(int take = 100)
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedOnUtc)
            .Take(take)
            .Select(x => new AuditLogListItemViewModel
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
