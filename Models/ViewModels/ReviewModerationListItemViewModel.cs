namespace ASP_PROJECT.Models.ViewModels;

public class ReviewModerationListItemViewModel
{
    public int ReviewId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public string VenueName { get; init; } = string.Empty;
    public string AuthorName { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public string ModerationStatus { get; init; } = string.Empty;
    public DateTime CreatedOnUtc { get; init; }
}
