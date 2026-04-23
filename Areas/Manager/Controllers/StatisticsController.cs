using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Manager.Controllers;

[Area("Manager")]
[Authorize(Roles = ASP_PROJECT.Data.DbInitializer.VenueManagerRole + "," + ASP_PROJECT.Data.DbInitializer.AdministratorRole)]
public class StatisticsController : Controller
{
    private readonly IManagementService _managementService;

    public StatisticsController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index(int? venueId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        var allowedVenueIds = await _managementService.GetAssignedVenueIdsAsync(userId);
        return View(await _managementService.GetVenueStatisticsAsync(allowedVenueIds, venueId));
    }
}
