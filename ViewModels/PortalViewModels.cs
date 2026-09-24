using System.ComponentModel.DataAnnotations;
using World_Consntrucoes.Models;

namespace World_Consntrucoes.ViewModels;

public class HomeIndexViewModel
{
    public IReadOnlyList<PropertyListing> Properties { get; set; } = Array.Empty<PropertyListing>();
    public string? Category { get; set; }
    public decimal? MaxPrice { get; set; }
}

public class InquiryInput
{
    public int PropertyListingId { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(3000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}

public class LoginInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class RegisterInput
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class PropertyFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = "Venda";

    [Range(0, 999999999)]
    public decimal Price { get; set; }

    [Required, StringLength(80)]
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
    public string? AssignedBrokerId { get; set; }
    public IReadOnlyList<ApplicationUser> Brokers { get; set; } = Array.Empty<ApplicationUser>();
}

public class AdminDashboardViewModel
{
    public int PropertyCount { get; set; }
    public int PublishedCount { get; set; }
    public int NewLeadCount { get; set; }
    public IReadOnlyList<PropertyListing> Properties { get; set; } = Array.Empty<PropertyListing>();
    public IReadOnlyList<ContactLead> RecentLeads { get; set; } = Array.Empty<ContactLead>();
}

public class AdminPropertyDetailsViewModel
{
    public PropertyListing Property { get; set; } = null!;
    public IReadOnlyList<ApplicationUser> Brokers { get; set; } = Array.Empty<ApplicationUser>();
    public IReadOnlyList<ApplicationUser> Clients { get; set; } = Array.Empty<ApplicationUser>();
}

public class CreateBrokerViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string TemporaryPassword { get; set; } = string.Empty;
}

public class LeadConversationViewModel
{
    public ContactLead Lead { get; set; } = null!;
}

public class ClientDashboardViewModel
{
    public IReadOnlyList<PropertyListing> Properties { get; set; } = Array.Empty<PropertyListing>();
    public IReadOnlyList<ContactLead> Leads { get; set; } = Array.Empty<ContactLead>();
}