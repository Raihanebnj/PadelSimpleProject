using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.Services;
public class DataService
{
    private readonly AppDbContext _db;
    public DataService(AppDbContext db) => _db = db;

    // --- Courts ---
    public Task<List<Court>> GetCourtsAsync() => _db.Courts.OrderBy(c => c.Name).ToListAsync();
    public async Task AddOrUpdateCourtAsync(Court c)
    {
        if (c.Id == 0) _db.Courts.Add(c); else _db.Courts.Update(c);
        await _db.SaveChangesAsync();
    }
    public async Task SoftDeleteCourtAsync(Court c)
    {
        c.IsDeleted = true; c.DeletedAt = DateTimeOffset.Now; _db.Courts.Update(c);
        await _db.SaveChangesAsync();
    }

    // --- Equipment ---
    public async Task<List<Equipment>> GetEquipmentAsync()
    {
        return await _db.Equipment
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }
    public async Task AddOrUpdateEquipmentAsync(Equipment e)
    {
        if (string.IsNullOrWhiteSpace(e.Name))
            throw new Exception("Naam mag niet leeg zijn.");

        if (e.Id == 0)
            _db.Equipment.Add(e);
        else
            _db.Equipment.Update(e);

        await _db.SaveChangesAsync();
    }
    public async Task SoftDeleteEquipmentAsync(Equipment e)
    {
        e.IsDeleted = true;
        e.DeletedAt = DateTimeOffset.Now;
        _db.Equipment.Update(e);
        await _db.SaveChangesAsync();
    }


    // --- Reservations ---
    public Task<List<Reservation>> GetReservationsAsync(DateTime? day = null)
    {
        var q = _db.Reservations
        .Include(r => r.Court)
        .Include(r => r.User)
        .Include(r => r.Equipment)
        .AsQueryable();

        if (day.HasValue)
        {
            var d = day.Value.Date;
            // LINQ query syntax example
            q = from r in q
                where r.StartTime.Date == d
                orderby r.StartTime
                select r;
        }
        else
        {
            // Method syntax example + lambda
            q = q.OrderBy(r => r.StartTime);
        }
        return q.ToListAsync();
    }

    public async Task<bool> CreateReservationAsync(Reservation r)
    {
        // Simple conflict check (overlap on same court)
        bool conflict = await _db.Reservations.AnyAsync(x => x.CourtId == r.CourtId &&
        ((r.StartTime < x.EndTime) && (x.StartTime < r.EndTime)));
        if (conflict) return false;

        // Equipment stock check
        if (r.EquipmentId.HasValue && r.EquipmentQuantity.HasValue)
        {
            var eq = await _db.Equipment.FindAsync(r.EquipmentId.Value);
            if (eq is null || eq.Quantity < r.EquipmentQuantity.Value) return false;
            eq.Quantity -= r.EquipmentQuantity.Value; // reserve stock
        }

        _db.Reservations.Add(r);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task CancelReservationAsync(Reservation r)
    {
        r.IsDeleted = true; r.DeletedAt = DateTimeOffset.Now;
        _db.Reservations.Update(r);
        await _db.SaveChangesAsync();
    }
}