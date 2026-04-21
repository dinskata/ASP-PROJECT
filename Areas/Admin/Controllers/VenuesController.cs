using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DbInitializer.AdministratorRole)]
public class VenuesController : Controller
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public async Task<IActionResult> Index(string? searchTerm, string? sortBy)
    {
        ViewBag.SearchTerm = searchTerm ?? string.Empty;
        ViewBag.SortBy = string.IsNullOrWhiteSpace(sortBy) ? "city" : sortBy;
        return View(await _venueService.GetAllForManagementAsync(null, searchTerm, sortBy));
    }

    public IActionResult Create()
    {
        return View(new VenueEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VenueEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var id = await _venueService.CreateAsync(model, GetActorId(), GetActorName());
        TempData["StatusMessage"] = "Venue created successfully.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _venueService.BuildEditorAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VenueEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _venueService.UpdateAsync(model, GetActorId(), GetActorName());
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Venue updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Administrator";
}
