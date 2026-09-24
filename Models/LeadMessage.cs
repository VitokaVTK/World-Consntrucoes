using System.ComponentModel.DataAnnotations;

namespace World_Consntrucoes.Models;

public class LeadMessage
{
    public int Id { get; set; }
    public int ContactLeadId { get; set; }
    public ContactLead ContactLead { get; set; } = null!;

    [StringLength(450)]
    public string? SenderUserId { get; set; }
    public ApplicationUser? SenderUser { get; set; }

    [Required, StringLength(120)]
    public string SenderName { get; set; } = string.Empty;

    [Required, StringLength(3000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}