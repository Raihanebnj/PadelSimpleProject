using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Common;
using PadelSimple.Models.Domain;
using PadelSimple.Models.Identity;

namespace PadelSimple.Models.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- Soft delete filters (expliciet, makkelijk uit te leggen) ---
        builder.Entity<Court>()
            .HasQueryFilter(c => !c.IsDeleted);

        builder.Entity<Equipment>()
            .HasQueryFilter(e => !e.IsDeleted);

        builder.Entity<Reservation>()
            .HasQueryFilter(r => !r.IsDeleted);

        builder.Entity<AppUser>()
            .HasQueryFilter(u => !u.IsDeleted);

        // --- Relaties voor Reservation ---
        builder.Entity<Reservation>()
            .HasOne(r => r.Court)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CourtId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Seeding basisdata ---
        Seed(builder);
    }

    private void Seed(ModelBuilder builder)
    {
        // Rollen
        var adminRole = new AppRole { Id = "ROLE_ADMIN", Name = "Admin", NormalizedName = "ADMIN" };
        var staffRole = new AppRole { Id = "ROLE_STAFF", Name = "Staff", NormalizedName = "STAFF" };
        var memberRole = new AppRole { Id = "ROLE_MEMBER", Name = "Member", NormalizedName = "MEMBER" };

        builder.Entity<AppRole>().HasData(adminRole, staffRole, memberRole);

        // Dummy courts
        builder.Entity<Court>().HasData(
            new Court { Id = 1, Name = "Court 1", Capacity = 4, IsIndoor = false, IsDeleted = false },
            new Court { Id = 2, Name = "Court 2", Capacity = 4, IsIndoor = true, IsDeleted = false }
        );

        // Dummy equipment
        builder.Entity<Equipment>().HasData(
            new Equipment
            {
                Id = 1,
                Name = "Padelracket",
                TotalQuantity = 20,
                AvailableQuantity = 20,
                IsActive = true,
                IsDeleted = false
            },
            new Equipment
            {
                Id = 2,
                Name = "Ballen set",
                TotalQuantity = 30,
                AvailableQuantity = 30,
                IsActive = true,
                IsDeleted = false
            }
        );
    }
}
