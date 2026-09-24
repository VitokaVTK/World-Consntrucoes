using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using World_Consntrucoes.Models;

namespace World_Consntrucoes.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<PropertyListing> PropertyListings => Set<PropertyListing>();
    public DbSet<PropertyPhoto> PropertyPhotos => Set<PropertyPhoto>();
    public DbSet<ConstructionUpdate> ConstructionUpdates => Set<ConstructionUpdate>();
    public DbSet<ClientPropertyAccess> ClientPropertyAccesses => Set<ClientPropertyAccess>();
    public DbSet<ContactLead> ContactLeads => Set<ContactLead>();
    public DbSet<LeadMessage> LeadMessages => Set<LeadMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PropertyListing>().Property(x => x.Price).HasPrecision(18, 2);
        builder.Entity<PropertyListing>().Property(x => x.AreaM2).HasPrecision(10, 2);
        builder.Entity<PropertyListing>()
            .HasOne(x => x.AssignedBroker)
            .WithMany()
            .HasForeignKey(x => x.AssignedBrokerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ClientPropertyAccess>()
            .HasKey(x => new { x.ClientId, x.PropertyListingId });
        builder.Entity<ClientPropertyAccess>()
            .HasOne(x => x.Client).WithMany()
            .HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ClientPropertyAccess>()
            .HasOne(x => x.PropertyListing).WithMany(x => x.ClientAccess)
            .HasForeignKey(x => x.PropertyListingId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ContactLead>()
            .HasOne(x => x.ClientUser).WithMany()
            .HasForeignKey(x => x.ClientUserId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<ContactLead>()
            .HasOne(x => x.AssignedBroker).WithMany()
            .HasForeignKey(x => x.AssignedBrokerId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<ContactLead>()
            .HasOne(x => x.PropertyListing).WithMany(x => x.Leads)
            .HasForeignKey(x => x.PropertyListingId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadMessage>()
            .HasOne(x => x.SenderUser).WithMany()
            .HasForeignKey(x => x.SenderUserId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<LeadMessage>()
            .HasOne(x => x.ContactLead).WithMany(x => x.Messages)
            .HasForeignKey(x => x.ContactLeadId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ConstructionUpdate>()
            .HasOne(x => x.Photo).WithMany()
            .HasForeignKey(x => x.PhotoId).OnDelete(DeleteBehavior.SetNull);
    }
}