using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class ReviewableEventViewModel
{
    public int EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public string Venue { get; init; } = string.Empty;
    public DateTime StartsAtUtc { get; init; }
    public bool HasExistingReview { get; init; }
    public string ExistingComment { get; init; } = string.Empty;
    public int ExistingRating { get; init; }
}
