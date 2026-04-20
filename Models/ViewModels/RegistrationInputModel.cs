using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models.ViewModels;

public class RegistrationInputModel
{
    public int EventId { get; set; }

    [Range(1, 10)]
    public int Tickets { get; set; } = 1;
}
