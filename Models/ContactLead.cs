using System.ComponentModel.DataAnnotations;

namespace World_Consntrucoes.Models;

public class ContactLead
{
    public int Id { get; set; }
    public int PropertyListingId { get; set; }
    public PropertyListing PropertyListing { get; set; } = null!;

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(450)]
    public string? ClientUserId { get; set; }
    public ApplicationUser? ClientUser { get; set; }

    [StringLength(450)]
    public string? AssignedBrokerId { get; set; }
    public ApplicationUser? AssignedBroker { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Novo";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<LeadMessage> Messages { get; set; } = new List<LeadMessage>();
}