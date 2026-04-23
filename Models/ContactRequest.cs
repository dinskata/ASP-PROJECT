using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class ContactRequest
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(1500, MinimumLength = 20)]
    public string Message { get; set; } = string.Empty;

    [StringLength(40)]
    public string Status { get; set; } = ContactRequestStatuses.Open;

    [StringLength(1000)]
    public string InternalNote { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedOnUtc { get; set; }

    [StringLength(80)]
    public string ResolvedByName { get; set; } = string.Empty;
}
