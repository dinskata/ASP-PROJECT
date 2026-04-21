namespace ASP_PROJECT.Models.ViewModels;

public class ModeratorDashboardViewModel
{
    public int PendingReviewsCount { get; init; }
    public int ApprovedReviewsCount { get; init; }
    public int VenueManagersCount { get; init; }
    public IReadOnlyCollection<ReviewModerationListItemViewModel> RecentReviews { get; init; } = Array.Empty<ReviewModerationListItemViewModel>();
}
