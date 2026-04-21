using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Manager.Controllers;

[Area("Manager")]
[Authorize(Roles = DbInitializer.VenueManagerRole + "," + DbInitializer.AdministratorRole)]
public class VenuesController : Controller
{
    private readonly IVenueService _venueService;
    private readonly IManagementService _managementService;

    public VenuesController(IVenueService venueService, IManagementService managementService)
    {
        _venueService = venueService;
        _managementService = managementService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _venueService.GetAllForManagementAsync(await GetAllowedVenueIdsAsync()));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _venueService.BuildEditorAsync(id, await GetAllowedVenueIdsAsync());
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

        var updated = await _venueService.UpdateAsync(model, GetActorId(), GetActorName(), await GetAllowedVenueIdsAsync());
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Venue updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    private async Task<IReadOnlyCollection<int>> GetAllowedVenueIdsAsync()
    {
        var userId = GetActorId();
        return userId is null ? Array.Empty<int>() : await _managementService.GetAssignedVenueIdsAsync(userId);
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Venue Manager";
}
