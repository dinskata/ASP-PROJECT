namespace ASP_PROJECT.Models.ViewModels;

public class AnnouncementListViewModel
{
    public IReadOnlyCollection<Announcement> Pinned { get; init; } = Array.Empty<Announcement>();
    public IReadOnlyCollection<Announcement> Recent { get; init; } = Array.Empty<Announcement>();
}
