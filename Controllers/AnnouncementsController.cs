using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Controllers;

public class AnnouncementsController : Controller
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _announcementService.GetAllAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var announcement = await _announcementService.GetByIdAsync(id);
        if (announcement is null)
        {
            return NotFound();
        }

        return View(announcement);
    }
}
