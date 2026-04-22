using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Controllers;

[Authorize(Roles = DbInitializer.VenueManagerRole + "," + DbInitializer.VenueStaffRole + "," + DbInitializer.SiteModeratorRole + "," + DbInitializer.AdministratorRole)]
public class TicketVerificationController : Controller
{
    private readonly IManagementService _managementService;

    public TicketVerificationController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await BuildPageModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(TicketVerificationPageViewModel model)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var hasGlobalAccess = User.IsInRole(DbInitializer.SiteModeratorRole) || User.IsInRole(DbInitializer.AdministratorRole);
        var allowedVenueIds = hasGlobalAccess || string.IsNullOrWhiteSpace(userId)
            ? null
            : await _managementService.GetAssignedVenueIdsAsync(userId);

        var result = await _managementService.VerifyTicketAsync(model.TicketCode, model.VerificationCode, allowedVenueIds, hasGlobalAccess);
        return View(await BuildPageModelAsync(model.TicketCode, model.VerificationCode, result));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(string ticketCode)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var hasGlobalAccess = User.IsInRole(DbInitializer.SiteModeratorRole) || User.IsInRole(DbInitializer.AdministratorRole);
        var allowedVenueIds = hasGlobalAccess || string.IsNullOrWhiteSpace(userId)
            ? null
            : await _managementService.GetAssignedVenueIdsAsync(userId);

        var result = await _managementService.MarkTicketCheckedInAsync(ticketCode, allowedVenueIds, hasGlobalAccess, userId, GetActorName());
        return View("Index", await BuildPageModelAsync(ticketCode, result?.VerificationCode, result));
    }

    private async Task<TicketVerificationPageViewModel> BuildPageModelAsync(
        string? ticketCode = null,
        string? verificationCode = null,
        TicketVerificationResultViewModel? result = null)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var hasGlobalAccess = User.IsInRole(DbInitializer.SiteModeratorRole) || User.IsInRole(DbInitializer.AdministratorRole);
        var allowedVenueIds = hasGlobalAccess || string.IsNullOrWhiteSpace(userId)
            ? null
            : await _managementService.GetAssignedVenueIdsAsync(userId);

        return new TicketVerificationPageViewModel
        {
            TicketCode = ticketCode ?? string.Empty,
            VerificationCode = verificationCode ?? string.Empty,
            Result = result,
            DemoTickets = await _managementService.GetTicketRegistryAsync(sortBy: "newest", take: 8, allowedVenueIds: allowedVenueIds),
            RecentTickets = await _managementService.GetTicketRegistryAsync(sortBy: "newest", take: 40, allowedVenueIds: allowedVenueIds)
        };
    }

    private string GetActorName()
        => User.Identity?.Name ?? "Ticket verifier";
}
