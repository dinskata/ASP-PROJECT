using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator")]
public class EventsController : Controller
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<IActionResult> Index(string? searchTerm, string? statusFilter, string? sortBy)
    {
        ViewBag.SearchTerm = searchTerm ?? string.Empty;
        ViewBag.StatusFilter = string.IsNullOrWhiteSpace(statusFilter) ? "all" : statusFilter;
        ViewBag.SortBy = string.IsNullOrWhiteSpace(sortBy) ? "date" : sortBy;
        return View(await _eventService.GetManagementEventsAsync(searchTerm, null, statusFilter, sortBy));
    }

    public async Task<IActionResult> Create()
    {
        return View(await _eventService.BuildEditorAsync(null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _eventService.GetCategoriesAsync();
            model.Venues = await _eventService.GetVenuesAsync();
            return View(model);
        }

        var id = await _eventService.CreateAsync(model, GetActorId(), GetActorName());
        if (id == 0)
        {
            model.Categories = await _eventService.GetCategoriesAsync();
            model.Venues = await _eventService.GetVenuesAsync();
            ModelState.AddModelError(string.Empty, "Event could not be created.");
            return View(model);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _eventService.BuildEditorAsync(id);
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _eventService.GetCategoriesAsync();
            model.Venues = await _eventService.GetVenuesAsync();
            return View(model);
        }

        var updated = await _eventService.UpdateAsync(model, GetActorId(), GetActorName());
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Event updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Administrator";
}
