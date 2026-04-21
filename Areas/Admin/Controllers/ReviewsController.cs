using ASP_PROJECT.Models;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator")]
public class ReviewsController : Controller
{
    private readonly IManagementService _managementService;

    public ReviewsController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index(string? statusFilter)
    {
        ViewBag.StatusFilter = string.IsNullOrWhiteSpace(statusFilter) ? "all" : statusFilter;
        return View(await _managementService.GetReviewsAsync(statusFilter));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Moderate(int reviewId, string decision)
    {
        var success = await _managementService.ModerateReviewAsync(
            reviewId,
            string.Equals(decision, ReviewModerationStatuses.Approved, StringComparison.OrdinalIgnoreCase),
            GetActorId() ?? string.Empty,
            GetActorName());

        TempData["StatusMessage"] = success ? "Review moderation updated." : "Review could not be updated.";
        return RedirectToAction(nameof(Index));
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Administrator";
}
