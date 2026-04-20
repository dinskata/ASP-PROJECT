namespace ASP_PROJECT.Models.ViewModels;

public class ReviewSummaryViewModel
{
    public string AuthorName { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public DateTime CreatedOnUtc { get; init; }
}
