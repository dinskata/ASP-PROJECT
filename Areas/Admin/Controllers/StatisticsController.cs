using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator")]
public class StatisticsController : Controller
{
    private readonly IManagementService _managementService;

    public StatisticsController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index(int? venueId)
        => View(await _managementService.GetVenueStatisticsAsync(null, venueId));
}
