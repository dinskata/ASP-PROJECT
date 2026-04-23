namespace ASP_PROJECT.Models.ViewModels;

public class AnnouncementManagementItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public bool IsPinned { get; init; }
    public DateTime PublishedOnUtc { get; init; }
    public string ContentPreview { get; init; } = string.Empty;
}
