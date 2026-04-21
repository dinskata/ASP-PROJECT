using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator")]
public class AuditController : Controller
{
    private readonly IManagementService _managementService;

    public AuditController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _managementService.GetAuditLogsAsync());
    }
}
