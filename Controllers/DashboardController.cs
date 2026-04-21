using ASP_PROJECT.Services.Interfaces;
using ASP_PROJECT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = User.Identity?.Name ?? "User";
        if (userId is null)
        {
            return Challenge();
        }

        return View(await _dashboardService.GetUserDashboardAsync(userId, userName));
    }

    public async Task<IActionResult> Reviews([FromServices] IEventService eventService)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        return View(await eventService.GetReviewableEventsAsync(userId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview([FromServices] IEventService eventService, ReviewInputModel model)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        var success = await eventService.AddReviewAsync(userId, model);
        TempData["StatusMessage"] = success
            ? "Review submitted."
            : "Review could not be submitted. Only verified attendees can review ended events once.";

        return RedirectToAction(nameof(Reviews));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(int registrationId, [FromServices] IEventService eventService)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        var success = await eventService.RequestRefundAsync(userId, registrationId);
        TempData["StatusMessage"] = success
            ? "Refund requested successfully. Your payment has been marked as refunded."
            : "Refund could not be completed. Refunds are only available more than 48 hours before the event.";

        return RedirectToAction(nameof(Index));
    }
}
