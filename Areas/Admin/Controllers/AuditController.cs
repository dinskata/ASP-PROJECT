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

    public async Task<IActionResult> Index(string? searchTerm, string? entityFilter, string? sortBy, string? ticketSearchTerm, string? ticketStatusFilter, string? ticketSortBy)
    {
        ViewBag.SearchTerm = searchTerm ?? string.Empty;
        ViewBag.EntityFilter = string.IsNullOrWhiteSpace(entityFilter) ? "all" : entityFilter;
        ViewBag.SortBy = string.IsNullOrWhiteSpace(sortBy) ? "newest" : sortBy;
        ViewBag.TicketSearchTerm = ticketSearchTerm ?? string.Empty;
        ViewBag.TicketStatusFilter = string.IsNullOrWhiteSpace(ticketStatusFilter) ? "all" : ticketStatusFilter;
        ViewBag.TicketSortBy = string.IsNullOrWhiteSpace(ticketSortBy) ? "newest" : ticketSortBy;

        return View(new ASP_PROJECT.Models.ViewModels.AuditHistoryPageViewModel
        {
            Entries = await _managementService.GetAuditLogsAsync(searchTerm, entityFilter, sortBy),
            Tickets = await _managementService.GetTicketRegistryAsync(ticketSearchTerm, ticketStatusFilter, ticketSortBy)
        });
    }
}
