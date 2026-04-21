namespace ASP_PROJECT.Models.ViewModels;

public class AuditLogListItemViewModel
{
    public string EntityType { get; init; } = string.Empty;
    public string ActionType { get; init; } = string.Empty;
    public int? EntityId { get; init; }
    public string PerformedByName { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public DateTime CreatedOnUtc { get; init; }
}
