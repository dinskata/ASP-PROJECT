using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Manager.Controllers;

[Area("Manager")]
[Authorize(Roles = DbInitializer.VenueManagerRole + "," + DbInitializer.AdministratorRole)]
public class EventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly IManagementService _managementService;

    public EventsController(IEventService eventService, IManagementService managementService)
    {
        _eventService = eventService;
        _managementService = managementService;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        var venueIds = await GetAllowedVenueIdsAsync();
        return View(await _eventService.GetManagementEventsAsync(searchTerm, venueIds));
    }

    public async Task<IActionResult> Create()
    {
        return View(await _eventService.BuildEditorAsync(null, await GetAllowedVenueIdsAsync()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventEditViewModel model)
    {
        var venueIds = await GetAllowedVenueIdsAsync();
        if (!ModelState.IsValid)
        {
            model.Categories = await _eventService.GetCategoriesAsync();
            model.Venues = (await _eventService.BuildEditorAsync(null, venueIds)).Venues;
            return View(model);
        }

        var id = await _eventService.CreateAsync(model, GetActorId(), GetActorName(), venueIds);
        if (id == 0)
        {
            TempData["StatusMessage"] = "You can create events only for your assigned venues.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _eventService.BuildEditorAsync(id, await GetAllowedVenueIdsAsync());
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
        var venueIds = await GetAllowedVenueIdsAsync();
        if (!ModelState.IsValid)
        {
            model.Categories = await _eventService.GetCategoriesAsync();
            model.Venues = (await _eventService.BuildEditorAsync(model.Id, venueIds)).Venues;
            return View(model);
        }

        var updated = await _eventService.UpdateAsync(model, GetActorId(), GetActorName(), venueIds);
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Event updated successfully.";
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
