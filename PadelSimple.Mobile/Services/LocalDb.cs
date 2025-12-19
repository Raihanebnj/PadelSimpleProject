using SQLite;

namespace PadelSimple.Mobile.Services;

public class LocalDb
{
    private readonly SQLiteAsyncConnection _db;

    public LocalDb()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "padelsimple.mobile.db");
        _db = new SQLiteAsyncConnection(path);

        _db.CreateTableAsync<LocalReservationPending>().Wait();
    }

    public Task<List<LocalReservationPending>> GetPendingAsync()
        => _db.Table<LocalReservationPending>().ToListAsync();

    public Task InsertPendingAsync(LocalReservationPending item)
        => _db.InsertAsync(item);

    public Task DeletePendingAsync(int id)
        => _db.DeleteAsync<LocalReservationPending>(id);
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
