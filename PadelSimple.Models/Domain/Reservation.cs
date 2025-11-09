using PadelSimple.Models.Identity;

namespace PadelSimple.Models.Domain;

public class Reservation
{
    public int Id { get; set; }

    public int CourtId { get; set; }
    public Court Court { get; set; } = default!;

    public int? EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int? EquipmentQuantity { get; set; }

    // ✅ User relatie toevoegen
    public string UserId { get; set; } = default!;
    public AppUser User { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
}
