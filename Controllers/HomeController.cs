using System.Diagnostics;
using ASP_PROJECT.Models;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAnnouncementService _announcementService;
    private readonly IEventService _eventService;

    public HomeController(
        ILogger<HomeController> logger,
        IAnnouncementService announcementService,
        IEventService eventService)
    {
        _logger = logger;
        _announcementService = announcementService;
        _eventService = eventService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Announcements"] = await _announcementService.GetAllAsync();
        return View(await _eventService.GetPublishedEventsAsync(null, null, 1, 3));
    }

    public IActionResult Privacy() => View();

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    public IActionResult StatusCodePage(int code)
    {
        return code switch
        {
            404 => View("NotFound", new ErrorViewModel
            {
                StatusCode = 404,
                Title = "Page not found",
                Message = "The page you requested does not exist or may have been moved."
            }),
            _ => View("ServerError", new ErrorViewModel
            {
                StatusCode = code,
                Title = "Unexpected error",
                Message = "Something went wrong while processing your request."
            })
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("Unhandled exception for request {RequestId}", HttpContext.TraceIdentifier);
        return View("ServerError", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = 500,
            Title = "Unexpected error",
            Message = "Something went wrong while processing your request."
        });
    }
}
