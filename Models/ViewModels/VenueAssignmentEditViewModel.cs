using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class VenueAssignmentEditViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<SelectableVenueViewModel> AvailableVenues { get; set; } = new();
}
