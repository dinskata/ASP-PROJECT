using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Controllers;

public class EventsController : Controller
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? statusFilter, int page = 1)
    {
        return View(await _eventService.GetPublishedEventsAsync(searchTerm, categoryId, statusFilter, 1, 100));
    }

    public async Task<IActionResult> Details(int id)
    {
        var eventDetails = await _eventService.GetDetailsAsync(id);
        if (eventDetails is null)
        {
            return NotFound();
        }

        return View(eventDetails);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegistrationInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = model.EventId });
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        var success = await _eventService.RegisterAsync(userId, model);
        TempData["StatusMessage"] = success
            ? "Payment completed successfully. Your tickets have been purchased."
            : "Purchase could not be completed. Check ticket availability, card details, or whether the event has already started.";
        return RedirectToAction(nameof(Details), new { id = model.EventId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ReviewInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Review data is invalid.";
            return RedirectToAction(nameof(Details), new { id = model.EventId });
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        var success = await _eventService.AddReviewAsync(userId, model);
        TempData["StatusMessage"] = success
            ? "Review submitted."
            : "Only registered attendees can submit one review after the event has ended.";
        return RedirectToAction(nameof(Details), new { id = model.EventId });
    }
}
