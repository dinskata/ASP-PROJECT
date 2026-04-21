using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator")]
public class UsersController : Controller
{
    private readonly IManagementService _managementService;

    public UsersController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _managementService.GetUsersAsync());
    }

    public async Task<IActionResult> Edit(string id)
    {
        var model = await _managementService.BuildUserEditorAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserAdminEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var fallback = await _managementService.BuildUserEditorAsync(model.Id);
            if (fallback is null)
            {
                return NotFound();
            }

            fallback.FullName = model.FullName;
            fallback.Email = model.Email;
            fallback.IsBuyer = model.IsBuyer;
            fallback.IsVenueManager = model.IsVenueManager;
            fallback.IsSiteModerator = model.IsSiteModerator;
            fallback.IsAdministrator = model.IsAdministrator;
            fallback.AvailableVenues = model.AvailableVenues;
            return View(fallback);
        }

        var updated = await _managementService.UpdateUserAsync(model, GetActorId() ?? string.Empty, GetActorName());
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "User permissions updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Administrator";
}
