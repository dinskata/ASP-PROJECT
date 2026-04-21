using ASP_PROJECT.Data;
using ASP_PROJECT.Models.ViewModels;
using ASP_PROJECT.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PROJECT.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = DbInitializer.AdministratorRole + "," + DbInitializer.VenueManagerRole)]
public class VenuesController : Controller
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _venueService.GetAllForManagementAsync());
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _venueService.BuildEditorAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VenueEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _venueService.UpdateAsync(model);
        if (!updated)
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Venue updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }
}
