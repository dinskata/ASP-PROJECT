using System.ComponentModel.DataAnnotations;

namespace ASP_PROJECT.Models;

public class Venue
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    public string Address { get; set; } = string.Empty;

    [Range(10, 5000)]
    public int Capacity { get; set; }

    [StringLength(400)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
