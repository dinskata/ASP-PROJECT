using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class ReviewInputModel
{
    public int EventId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Comment { get; set; } = string.Empty;
}
