using System.ComponentModel.DataAnnotations;

namespace World_Consntrucoes.Models;

public class PropertyPhoto
{
    public int Id { get; set; }
    public int PropertyListingId { get; set; }
    public PropertyListing PropertyListing { get; set; } = null!;

    [Required, StringLength(80)]
    public string StorageFileName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string ContentType { get; set; } = string.Empty;

    [StringLength(160)]
    public string Caption { get; set; } = string.Empty;

    public bool IsPublic { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}