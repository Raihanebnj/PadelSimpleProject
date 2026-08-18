using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PadelSimple.Models.Common;

namespace PadelSimple.Models.Identity;

/// <summary>
/// Applicatiegebruiker met extra velden voor de padelclub.
/// </summary>
public class AppUser : IdentityUser, ISoftDeletable
{
    /// <summary>Voornaam van de gebruiker.</summary>
    [Required(ErrorMessage = "Voornaam is verplicht.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Voornaam moet tussen 1 en 50 tekens zijn.")]
    [Display(Name = "Voornaam")]
    public string Voornaam { get; set; } = string.Empty;

    /// <summary>Achternaam van de gebruiker.</summary>
    [Required(ErrorMessage = "Achternaam is verplicht.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Achternaam moet tussen 1 en 100 tekens zijn.")]
    [Display(Name = "Achternaam")]
    public string Achternaam { get; set; } = string.Empty;

    /// <summary>Telefoonnummer (niet verplicht).</summary>
    [StringLength(20, ErrorMessage = "Telefoonnummer mag maximaal 20 tekens zijn.")]
    [Display(Name = "Telefoonnummer")]
    public string Telefoonnummer { get; set; } = string.Empty;

    /// <summary>Geeft aan of de gebruiker een betalend lid is.</summary>
    [Display(Name = "Lid")]
    public bool IsLid { get; set; }

    /// <summary>Geeft aan of de gebruiker geblokkeerd is.</summary>
    [Display(Name = "Geblokkeerd")]
    public bool IsBlocked { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigatie
    public ICollection<Domain.Reservation> Reservations { get; set; } = new List<Domain.Reservation>();

    // Alias voor compatibiliteit
    public bool IsMember
    {
        get => IsLid;
        set => IsLid = value;
    }

    /// <summary>Volledige naam (voornaam + achternaam).</summary>
    public string VolledigeNaam => $"{Voornaam} {Achternaam}".Trim();
}
