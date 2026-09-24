namespace World_Consntrucoes.Models;

public class ClientPropertyAccess
{
    public string ClientId { get; set; } = string.Empty;
    public ApplicationUser Client { get; set; } = null!;
    public int PropertyListingId { get; set; }
    public PropertyListing PropertyListing { get; set; } = null!;
}