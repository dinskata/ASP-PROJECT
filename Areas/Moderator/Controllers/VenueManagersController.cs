using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Moderator.Controllers;

[Area("Moderator")]
[Authorize(Roles = DbInitializer.SiteModeratorRole + "," + DbInitializer.AdministratorRole)]
public class VenueManagersController : Controller
{
    private readonly IManagementService _managementService;

    public VenueManagersController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _managementService.GetVenueManagersAsync());
    }

    public async Task<IActionResult> Edit(string id)
    {
        var model = await _managementService.BuildVenueAssignmentsAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VenueAssignmentEditViewModel model)
    {
        var updated = await _managementService.UpdateVenueAssignmentsAsync(model, GetActorId() ?? string.Empty, GetActorName());
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Venue assignments updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.UserId });
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Moderator";
}
