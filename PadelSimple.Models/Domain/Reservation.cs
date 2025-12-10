using PadelSimple.Models.Common;
using PadelSimple.Models.Identity;

namespace PadelSimple.Models.Domain;

public class Reservation : ISoftDeletable
{
    public int Id { get; set; }

    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int NumberOfPlayers { get; set; }

    // Eén soort materiaal per reservatie (simpel model)
    public int? EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }
    public int? EquipmentQuantity { get; set; }

    // Court
    public int CourtId { get; set; }
    public Court Court { get; set; } = null!;

    // User
    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
