using ASP_PROJECT.Data;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Manager.Controllers;

[Area("Manager")]
[Authorize(Roles = DbInitializer.VenueManagerRole + "," + DbInitializer.AdministratorRole)]
public class DashboardController : Controller
{
    private readonly IManagementService _managementService;

    public DashboardController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Challenge();
        }

        return View(await _managementService.GetManagerDashboardAsync(userId));
    }
}
