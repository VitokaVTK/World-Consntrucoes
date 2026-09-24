using System.ComponentModel.DataAnnotations;

namespace World_Consntrucoes.Models;

public class ConstructionUpdate
{
    public int Id { get; set; }
    public int PropertyListingId { get; set; }
    public PropertyListing PropertyListing { get; set; } = null!;

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    public int? PhotoId { get; set; }
    public PropertyPhoto? Photo { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}