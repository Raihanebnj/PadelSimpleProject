using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext).GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { builder });
            }
        }

        builder.Entity<Reservation>()
        .HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Reservation>()
        .HasOne(r => r.Court)
        .WithMany()
        .HasForeignKey(r => r.CourtId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Reservation>()
        .HasOne(r => r.Equipment)
        .WithMany()
        .HasForeignKey(r => r.EquipmentId)
        .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : class, ISoftDeletable
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
