using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class ContactRequestUpdateViewModel
{
    public int Id { get; set; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedOnUtc { get; init; }
    public DateTime? ResolvedOnUtc { get; init; }
    public string ResolvedByName { get; init; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string Status { get; set; } = ContactRequestStatuses.Open;

    [StringLength(1000)]
    [Display(Name = "Internal note")]
    public string InternalNote { get; set; } = string.Empty;
}
