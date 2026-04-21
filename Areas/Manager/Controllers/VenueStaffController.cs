using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Manager.Controllers;

[Area("Manager")]
[Authorize(Roles = DbInitializer.VenueManagerRole + "," + DbInitializer.AdministratorRole)]
public class VenueStaffController : Controller
{
    private readonly IManagementService _managementService;

    public VenueStaffController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index()
    {
        var managerUserId = GetActorId();
        if (string.IsNullOrWhiteSpace(managerUserId))
        {
            return Challenge();
        }

        return View(await _managementService.GetVenueStaffAsync(managerUserId));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var managerUserId = GetActorId();
        if (string.IsNullOrWhiteSpace(managerUserId))
        {
            return Challenge();
        }

        return View(await _managementService.BuildVenueStaffCreateAsync(managerUserId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VenueStaffCreateViewModel model)
    {
        var managerUserId = GetActorId();
        if (string.IsNullOrWhiteSpace(managerUserId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(await _managementService.BuildVenueStaffCreateAsync(managerUserId, model));
        }

        var result = await _managementService.CreateVenueStaffAsync(model, managerUserId, managerUserId, GetActorName());
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create venue staff.");
            return View(await _managementService.BuildVenueStaffCreateAsync(managerUserId, model));
        }

        TempData["StatusMessage"] = "Venue staff account created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var managerUserId = GetActorId();
        if (string.IsNullOrWhiteSpace(managerUserId))
        {
            return Challenge();
        }

        var model = await _managementService.BuildVenueStaffAssignmentsAsync(id, managerUserId);
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
        var managerUserId = GetActorId();
        if (string.IsNullOrWhiteSpace(managerUserId))
        {
            return Challenge();
        }

        var updated = await _managementService.UpdateVenueStaffAssignmentsAsync(model, managerUserId, managerUserId, GetActorName());
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Venue staff assignments updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.UserId });
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Venue Manager";
}
