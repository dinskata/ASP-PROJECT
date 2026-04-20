namespace ASP_PROJECT.Models.ViewModels;

public class EventListViewModel
{
    public PagedResult<EventListItemViewModel> Result { get; init; } = new();
    public string SearchTerm { get; init; } = string.Empty;
    public int? CategoryId { get; init; }
    public IReadOnlyCollection<Category> Categories { get; init; } = Array.Empty<Category>();
}
