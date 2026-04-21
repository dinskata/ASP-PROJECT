namespace ASP_PROJECT.Models.ViewModels;

public class SelectableVenueViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public bool IsSelected { get; set; }
}
