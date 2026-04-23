using ASP_PROJECT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<RegistrationTicket> RegistrationTickets => Set<RegistrationTicket>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<UserVenueAssignment> UserVenueAssignments => Set<UserVenueAssignment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>()
            .HasIndex(x => x.Name)
            .IsUnique();

        builder.Entity<Registration>()
            .HasIndex(x => new { x.EventId, x.UserId })
            .IsUnique();

        builder.Entity<RegistrationTicket>()
            .HasIndex(x => new { x.RegistrationId, x.TicketNumber })
            .IsUnique();

        builder.Entity<RegistrationTicket>()
            .HasIndex(x => x.TicketCode)
            .IsUnique();

        builder.Entity<Review>()
            .HasIndex(x => new { x.EventId, x.UserId })
            .IsUnique();

        builder.Entity<UserVenueAssignment>()
            .HasIndex(x => new { x.UserId, x.VenueId })
            .IsUnique();

        builder.Entity<ContactRequest>()
            .HasIndex(x => new { x.Status, x.CreatedOnUtc });

        builder.Entity<Event>()
            .Property(x => x.Price)
            .HasColumnType("decimal(10,2)");

        builder.Entity<Registration>()
            .Property(x => x.AmountPaid)
            .HasColumnType("decimal(10,2)");

        builder.Entity<RegistrationTicket>()
            .HasOne(x => x.Registration)
            .WithMany(x => x.RegistrationTickets)
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Registration>()
            .HasOne(x => x.User)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(x => x.User)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserVenueAssignment>()
            .HasOne(x => x.User)
            .WithMany(x => x.VenueAssignments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserVenueAssignment>()
            .HasOne(x => x.Venue)
            .WithMany(x => x.UserAssignments)
            .HasForeignKey(x => x.VenueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
