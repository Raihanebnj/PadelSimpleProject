using PadelSimple.Models.Common;

namespace PadelSimple.Models.Domain;

public class Equipment : ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
