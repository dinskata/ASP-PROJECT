using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class RegistrationInputModel
{
    public int EventId { get; set; }

    [Range(1, 10)]
    public int Tickets { get; set; } = 1;

    [Required]
    [StringLength(80, MinimumLength = 3)]
    [Display(Name = "Cardholder name")]
    public string CardholderName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must contain exactly 16 digits.")]
    [Display(Name = "Card number")]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    [Display(Name = "Expiry month")]
    public int ExpiryMonth { get; set; }

    [Range(2026, 2100)]
    [Display(Name = "Expiry year")]
    public int ExpiryYear { get; set; }

    [Required]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Security code must contain 3 or 4 digits.")]
    [Display(Name = "Security code")]
    public string Cvv { get; set; } = string.Empty;

    public string PaymentDecision { get; set; } = string.Empty;
}
