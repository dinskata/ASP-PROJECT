using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator")]
public class PaymentsController : Controller
{
    private readonly IManagementService _managementService;

    public PaymentsController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _managementService.GetPaymentsAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(int registrationId)
    {
        var success = await _managementService.ForceRefundAsync(registrationId, GetActorId() ?? string.Empty, GetActorName());
        TempData["StatusMessage"] = success ? "Payment refunded successfully." : "Payment could not be refunded.";
        return RedirectToAction(nameof(Index));
    }

    private string? GetActorId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    private string GetActorName()
        => User.Identity?.Name ?? "Administrator";
}
