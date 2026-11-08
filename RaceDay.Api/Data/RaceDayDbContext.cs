using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Role ----
        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(r => r.RoleName).IsUnique();
        });

        // ---- User ----
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();

            e.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Event ----
        modelBuilder.Entity<Event>(e =>
        {
            e.Property(ev => ev.EventType).HasConversion<string>().HasMaxLength(20);
        });

        // ---- EventCategory (belongs to one Event — Part 2 reconciliation) ----
        modelBuilder.Entity<EventCategory>(e =>
        {
            e.HasOne(c => c.Event)
                .WithMany(ev => ev.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // A category name must be unique within its own event, not globally.
            e.HasIndex(c => new { c.EventId, c.CategoryName }).IsUnique();
        });

        // ---- Enrolment ----
        modelBuilder.Entity<Enrolment>(e =>
        {
            e.HasOne(en => en.User)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(en => en.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(en => en.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(en => en.EventCategory)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(en => en.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // A participant cannot enrol in the same event more than once.
            e.HasIndex(en => new { en.UserId, en.EventId }).IsUnique();
        });

        // ---- Result ----
        modelBuilder.Entity<Result>(e =>
        {
            e.HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Restrict); // never lose historical results via cascade

            e.HasIndex(r => r.EnrolmentId).IsUnique();
        });
    }
}
