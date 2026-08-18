using Microsoft.AspNetCore.Identity;
using PadelSimple.Models.Common;
using PadelSimple.Models.Domain;

namespace PadelSimple.Models.Identity;

/// <summary>
/// Applicatiegebruiker met extra velden voor de padelclub.
/// </summary>
public class AppUser : IdentityUser, ISoftDeletable
{
    /// <summary>Voornaam van de gebruiker.</summary>
    public string Voornaam { get; set; } = string.Empty;

    /// <summary>Achternaam van de gebruiker.</summary>
    public string Achternaam { get; set; } = string.Empty;

    /// <summary>Telefoonnummer (niet verplicht).</summary>
    public string Telefoonnummer { get; set; } = string.Empty;

    /// <summary>Geeft aan of de gebruiker een betalend lid is.</summary>
    public bool IsLid { get; set; }

    /// <summary>Geeft aan of de gebruiker geblokkeerd is (legacy veld).</summary>
    public bool IsBlocked { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigatie
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    // Alias voor compatibiliteit
    public bool IsMember
    {
        get => IsLid;
        set => IsLid = value;
    }
}
