using PadelSimple.Models.Dtos;
using SQLite;

namespace PadelSimple.Mobile.Services;

public class LocalDb
{
    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    public LocalDb()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "padelsimple.mobile.db");
        _db = new SQLiteAsyncConnection(path);
    }

    private async Task InitAsync()
    {
        if (_initialized) return;

        await _db.CreateTableAsync<LocalReservationPending>();
        await _db.CreateTableAsync<LocalCachedCourt>();
        await _db.CreateTableAsync<LocalCachedEquipment>();
        await _db.CreateTableAsync<LocalCachedReservation>();

        _initialized = true;
    }
    public async Task<List<LocalReservationPending>> GetPendingAsync()
    {
        await InitAsync();
        return await _db.Table<LocalReservationPending>().ToListAsync();
    }

    public async Task InsertPendingAsync(LocalReservationPending item)
    {
        await InitAsync();
        await _db.InsertAsync(item);
    }

    public async Task DeletePendingAsync(int id)
    {
        await InitAsync();
        await _db.DeleteAsync<LocalReservationPending>(id);
    }
    
    public async Task ReplaceCourtsAsync(IEnumerable<CourtDto> courts)
    {
        await InitAsync();
        await _db.DeleteAllAsync<LocalCachedCourt>();
        foreach (var c in courts)
        {
            await _db.InsertAsync(new LocalCachedCourt
            {
                Id = c.Id,
                Name = c.Name,
                Capacity = c.Capacity,
                IsIndoor = c.IsIndoor
            });
        }
    }

    public async Task<List<CourtDto>> GetCachedCourtsAsync()
    {
        await InitAsync();
        var rows = await _db.Table<LocalCachedCourt>().ToListAsync();
        return rows
            .Select(r => new CourtDto(r.Id, r.Name, r.Capacity, r.IsIndoor))
            .ToList();
    }

    public async Task ReplaceEquipmentAsync(IEnumerable<EquipmentDto> items)
    {
        await InitAsync();
        await _db.DeleteAllAsync<LocalCachedEquipment>();
        foreach (var e in items)
        {
            await _db.InsertAsync(new LocalCachedEquipment
            {
                Id = e.Id,
                Name = e.Name,
                TotalQuantity = e.TotalQuantity,
                AvailableQuantity = e.AvailableQuantity,
                IsActive = e.IsActive
            });
        }
    }

    public async Task<List<EquipmentDto>> GetCachedEquipmentAsync()
    {
        await InitAsync();
        var rows = await _db.Table<LocalCachedEquipment>().ToListAsync();
        return rows
            .Select(r => new EquipmentDto(r.Id, r.Name, r.TotalQuantity, r.AvailableQuantity, r.IsActive))
            .ToList();
    }

    public async Task ReplaceReservationsAsync(DateTime date, IEnumerable<ReservationDto> items)
    {
        await InitAsync();
        await _db.ExecuteAsync("DELETE FROM LocalCachedReservation WHERE Date = ?", date.Date);
        foreach (var r in items)
        {
            await _db.InsertAsync(new LocalCachedReservation
            {
                Id = r.Id,
                CourtId = r.CourtId,
                CourtName = r.CourtName,
                Date = r.Date.Date,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                NumberOfPlayers = r.NumberOfPlayers,
                EquipmentId = r.EquipmentId,
                EquipmentName = r.EquipmentName,
                EquipmentQuantity = r.EquipmentQuantity
            });
        }
    }

    public async Task<List<ReservationDto>> GetCachedReservationsAsync(DateTime date)
    {
        await InitAsync();
        var rows = await _db
            .Table<LocalCachedReservation>()
            .Where(x => x.Date == date.Date)
            .ToListAsync();
        return rows
            .Select(r => new ReservationDto(
                r.Id,
                r.CourtId,
                r.CourtName,
                r.Date,
                r.StartTime,
                r.EndTime,
                r.NumberOfPlayers,
                r.EquipmentId,
                r.EquipmentName,
                r.EquipmentQuantity))
            .ToList();
    }
}

public class LocalReservationPending
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int CourtId { get; set; }
    public DateTime Date { get; set; }

    // opslaan als tekst voor simpliciteit
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";

    public int NumberOfPlayers { get; set; }
    public int? EquipmentId { get; set; }
    public int? EquipmentQuantity { get; set; }
}

public class LocalCachedCourt
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Capacity { get; set; }
    public bool IsIndoor { get; set; }
}

public class LocalCachedEquipment
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class LocalCachedReservation
{
    [PrimaryKey]
    public int Id { get; set; }
    public int CourtId { get; set; }
    public string CourtName { get; set; } = "";
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int NumberOfPlayers { get; set; }
    public int? EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    public int? EquipmentQuantity { get; set; }
}
