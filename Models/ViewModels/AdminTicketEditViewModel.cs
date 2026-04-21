using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class AdminTicketEditViewModel
{
    public int TicketId { get; set; }
    public int RegistrationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string EventTitle { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public DateTime StartsAtUtc { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string SeatLabel { get; set; } = string.Empty;

    [StringLength(180)]
    public string TicketNote { get; set; } = string.Empty;
}
