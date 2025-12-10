using Microsoft.AspNetCore.Identity;
using PadelSimple.Models.Common;
using PadelSimple.Models.Domain;

namespace PadelSimple.Models.Identity;

public class AppUser : IdentityUser, ISoftDeletable
{
    // Extra eigenschap – verplicht volgens opdracht
    public bool IsMember { get; set; }

    public bool IsBlocked { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigatie
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
