using ASP_PROJECT.Data;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Moderator.Controllers;

[Area("Moderator")]
[Authorize(Roles = DbInitializer.SiteModeratorRole + "," + DbInitializer.AdministratorRole)]
public class DashboardController : Controller
{
    private readonly IManagementService _managementService;

    public DashboardController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _managementService.GetModeratorDashboardAsync());
    }
}
