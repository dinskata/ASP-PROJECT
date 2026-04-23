using System.Diagnostics;
using ASP_PROJECT.Models;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAnnouncementService _announcementService;
    private readonly IEventService _eventService;
    private readonly IContactRequestService _contactRequestService;

    public HomeController(
        ILogger<HomeController> logger,
        IAnnouncementService announcementService,
        IEventService eventService,
        IContactRequestService contactRequestService)
    {
        _logger = logger;
        _announcementService = announcementService;
        _eventService = eventService;
        _contactRequestService = contactRequestService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Announcements"] = await _announcementService.GetAllAsync();
        return View(await _eventService.GetPublishedEventsAsync(null, null, "upcoming", 1, 3));
    }

    public IActionResult Privacy() => View();

    public IActionResult About() => View();

    [HttpGet]
    public IActionResult Contact() => View(new ContactFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _contactRequestService.CreateAsync(model);
        TempData["StatusMessage"] = "Your message has been sent successfully.";
        ModelState.Clear();
        return View(new ContactFormViewModel());
    }

    public IActionResult StatusCodePage(int code)
    {
        return code switch
        {
            400 => View("BadRequest", new ErrorViewModel
            {
                StatusCode = 400,
                Title = "Bad request",
                Message = "The request could not be processed. Please go back and try again."
            }),
            404 => View("NotFound", new ErrorViewModel
            {
                StatusCode = 404,
                Title = "Page not found",
                Message = "The page you requested does not exist or may have been moved."
            }),
            500 => View("ServerError", new ErrorViewModel
            {
                StatusCode = 500,
                Title = "Server error",
                Message = "Something went wrong while processing your request."
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
