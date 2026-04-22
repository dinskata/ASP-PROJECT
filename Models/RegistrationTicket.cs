using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class RegistrationTicket
{
    public int Id { get; set; }

    public int RegistrationId { get; set; }
    public Registration? Registration { get; set; }

    [Range(1, 50)]
    public int TicketNumber { get; set; }

    [Required]
    [StringLength(24)]
    public string TicketCode { get; set; } = string.Empty;

    [Required]
    [StringLength(12)]
    public string VerificationCode { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string SeatLabel { get; set; } = string.Empty;

    [StringLength(180)]
    public string TicketNote { get; set; } = string.Empty;

    public bool IsCheckedIn { get; set; }

    public DateTime? CheckedInOnUtc { get; set; }

    [StringLength(160)]
    public string CheckedInByName { get; set; } = string.Empty;

    public DateTime IssuedOnUtc { get; set; } = DateTime.UtcNow;
}
