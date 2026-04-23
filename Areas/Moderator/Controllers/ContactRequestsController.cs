using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Moderator.Controllers;

[Area("Moderator")]
[Authorize(Roles = DbInitializer.SiteModeratorRole + "," + DbInitializer.AdministratorRole)]
public class ContactRequestsController : Controller
{
    private readonly IContactRequestService _contactRequestService;

    public ContactRequestsController(IContactRequestService contactRequestService)
    {
        _contactRequestService = contactRequestService;
    }

    public async Task<IActionResult> Index(string? statusFilter, string? searchTerm, string? sortBy)
    {
        ViewBag.StatusFilter = string.IsNullOrWhiteSpace(statusFilter) ? "all" : statusFilter;
        ViewBag.SearchTerm = searchTerm ?? string.Empty;
        ViewBag.SortBy = string.IsNullOrWhiteSpace(sortBy) ? "newest" : sortBy;
        return View(await _contactRequestService.GetManagementAsync(statusFilter, searchTerm, sortBy));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _contactRequestService.BuildUpdateModelAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Details(ContactRequestUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _contactRequestService.UpdateAsync(model, GetActorId(), GetActorName());
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Contact request updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Moderator";
}
