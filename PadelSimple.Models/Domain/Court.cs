using PadelSimple.Models.Common;

namespace PadelSimple.Models.Domain;

public class Court : ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public bool IsIndoor { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

