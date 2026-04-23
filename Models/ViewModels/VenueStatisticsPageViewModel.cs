namespace ASP_PROJECT.Models.ViewModels;

public class VenueStatisticsPageViewModel
{
    public int? SelectedVenueId { get; init; }
    public IReadOnlyCollection<SelectableVenueViewModel> AvailableVenues { get; init; } = Array.Empty<SelectableVenueViewModel>();
    public VenueStatisticsSummaryViewModel? SelectedVenue { get; init; }
}
