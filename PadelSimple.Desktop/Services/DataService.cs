using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.Services;

public class DataService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DataService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Court>> GetCourtsAsync()
    {
        using var db = _contextFactory.CreateDbContext();
        return await db.Courts
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Equipment>> GetEquipmentAsync()
    {
        using var db = _contextFactory.CreateDbContext();

        var query =
            from e in db.Equipment
            where e.IsActive
            orderby e.Name
            select e;

        return await query.ToListAsync();
    }

    // Eerst uit DB halen, daarna in geheugen sorteren (TimeSpan -> geen probleem)
    public async Task<List<Reservation>> GetReservationsAsync(DateTime? forDate = null)
    {
        using var db = _contextFactory.CreateDbContext();

        var query = db.Reservations
            .Include(r => r.Court)
            .Include(r => r.Equipment)
            .Include(r => r.User)
            .AsQueryable();

        if (forDate.HasValue)
            query = query.Where(r => r.Date.Date == forDate.Value.Date);

        var list = await query.ToListAsync();

        return list
            .OrderBy(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToList();
    }

    public async Task CreateReservationAsync(Reservation reservation)
    {
        using var db = _contextFactory.CreateDbContext();
        using var tx = await db.Database.BeginTransactionAsync();

        try
        {
            // Eerst alle reservaties voor zelfde terrein & datum ophalen
            var sameCourtReservations = await db.Reservations
                .Where(r => r.CourtId == reservation.CourtId &&
                            r.Date == reservation.Date)
                .ToListAsync();

            // Overlap-check in geheugen (LINQ to Objects → TimeSpan ok)
            bool overlap = sameCourtReservations.Any(r =>
                r.StartTime < reservation.EndTime &&
                reservation.StartTime < r.EndTime);

            if (overlap)
                throw new InvalidOperationException("Er bestaat al een reservatie voor dit terrein en tijdslot.");

            // Materiaal
            if (reservation.EquipmentId.HasValue && reservation.EquipmentQuantity.HasValue)
            {
                var eq = await db.Equipment.FindAsync(reservation.EquipmentId.Value)
                         ?? throw new InvalidOperationException("Materiaal niet gevonden.");

                if (eq.AvailableQuantity < reservation.EquipmentQuantity.Value)
                    throw new InvalidOperationException("Niet genoeg materiaal beschikbaar.");

                eq.AvailableQuantity -= reservation.EquipmentQuantity.Value;
            }

            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task SoftDeleteReservationAsync(int id)
    {
        using var db = _contextFactory.CreateDbContext();

        var res = await db.Reservations.FindAsync(id);
        if (res == null) return;

        res.IsDeleted = true;
        res.DeletedAt = DateTime.UtcNow;

        if (res.EquipmentId.HasValue && res.EquipmentQuantity.HasValue)
        {
            var eq = await db.Equipment.FindAsync(res.EquipmentId.Value);
            if (eq != null)
            {
                eq.AvailableQuantity += res.EquipmentQuantity.Value;
                if (eq.AvailableQuantity > eq.TotalQuantity)
                    eq.AvailableQuantity = eq.TotalQuantity;
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task SaveCourtAsync(Court court)
    {
        using var db = _contextFactory.CreateDbContext();

        if (court.Id == 0)
            db.Courts.Add(court);
        else
            db.Courts.Update(court);

        await db.SaveChangesAsync();
    }

    public async Task SaveEquipmentAsync(Equipment equipment)
    {
        using var db = _contextFactory.CreateDbContext();

        if (equipment.Id == 0)
            db.Equipment.Add(equipment);
        else
            db.Equipment.Update(equipment);

        await db.SaveChangesAsync();
    }

}
