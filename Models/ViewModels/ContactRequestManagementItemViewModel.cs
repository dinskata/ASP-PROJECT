namespace ASP_PROJECT.Models.ViewModels;

public class ContactRequestManagementItemViewModel
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedOnUtc { get; init; }
    public string MessagePreview { get; init; } = string.Empty;
}
