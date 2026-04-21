namespace ASP_PROJECT.Models.ViewModels;

public class UserAdminListItemViewModel
{
    public string Id { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedOnUtc { get; init; }
    public string RolesDisplay { get; init; } = string.Empty;
    public int AssignedVenuesCount { get; init; }
}
