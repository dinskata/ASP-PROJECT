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

    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, int page = 1)
    {
        return View(await _eventService.GetPublishedEventsAsync(searchTerm, categoryId, page, 10));
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

        var id = await _eventService.CreateAsync(model);
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

        var updated = await _eventService.UpdateAsync(model);
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Event updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }
}
