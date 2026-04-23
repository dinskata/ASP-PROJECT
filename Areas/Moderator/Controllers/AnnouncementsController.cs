using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Moderator.Controllers;

[Area("Moderator")]
[Authorize(Roles = DbInitializer.SiteModeratorRole + "," + DbInitializer.AdministratorRole)]
public class AnnouncementsController : Controller
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    public async Task<IActionResult> Index()
        => View(await _announcementService.GetManagementAsync());

    public async Task<IActionResult> Create()
        => View(await _announcementService.BuildEditorAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AnnouncementEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var id = await _announcementService.CreateAsync(model, GetActorId(), GetActorName());
        TempData["StatusMessage"] = "Announcement created successfully.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _announcementService.BuildEditorAsync(id);
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AnnouncementEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _announcementService.UpdateAsync(model, GetActorId(), GetActorName());
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Announcement updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Moderator";
}
