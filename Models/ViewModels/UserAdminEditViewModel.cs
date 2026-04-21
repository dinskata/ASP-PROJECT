using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class UserAdminEditViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public bool IsBuyer { get; set; }
    public bool IsVenueManager { get; set; }
    public bool IsSiteModerator { get; set; }
    public bool IsAdministrator { get; set; }

    public List<SelectableVenueViewModel> AvailableVenues { get; set; } = new();
}
