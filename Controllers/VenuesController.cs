using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Controllers;

public class VenuesController : Controller
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public async Task<IActionResult> Index(string? searchTerm, int page = 1)
    {
        ViewBag.SearchTerm = searchTerm;
        return View(await _venueService.GetPagedAsync(searchTerm, page, 6));
    }

    public async Task<IActionResult> Details(int id)
    {
        var venue = await _venueService.GetDetailsAsync(id);
        if (venue is null)
        {
            return NotFound();
        }

        return View(venue);
    }
}
