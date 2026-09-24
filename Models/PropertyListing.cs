using System.ComponentModel.DataAnnotations;

namespace World_Consntrucoes.Models;

public class PropertyListing
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Category { get; set; } = "Venda";

    [Range(0, 999999999)]
    public decimal Price { get; set; }

    [StringLength(80)]
    public string City { get; set; } = string.Empty;

    [StringLength(80)]
    public string Neighborhood { get; set; } = string.Empty;

    [Range(0, 30)]
    public int Bedrooms { get; set; }

    [Range(0, 30)]
    public int Bathrooms { get; set; }

    [Range(0, 30)]
    public int ParkingSpaces { get; set; }

    [Range(0, 10000)]
    public decimal AreaM2 { get; set; }

    public bool IsPublished { get; set; }

    [StringLength(450)]
    public string? AssignedBrokerId { get; set; }

    public ApplicationUser? AssignedBroker { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PropertyPhoto> Photos { get; set; } = new List<PropertyPhoto>();
    public ICollection<ConstructionUpdate> ConstructionUpdates { get; set; } = new List<ConstructionUpdate>();
    public ICollection<ClientPropertyAccess> ClientAccess { get; set; } = new List<ClientPropertyAccess>();
    public ICollection<ContactLead> Leads { get; set; } = new List<ContactLead>();
}