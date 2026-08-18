using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels.Auth;

public class RegisterVm
{
    [Required(ErrorMessage = "Voornaam is verplicht.")]
    [StringLength(50, ErrorMessage = "Voornaam mag maximaal 50 tekens lang zijn.")]
    [Display(Name = "Voornaam")]
    public string Voornaam { get; set; } = string.Empty;

    [Required(ErrorMessage = "Achternaam is verplicht.")]
    [StringLength(100, ErrorMessage = "Achternaam mag maximaal 100 tekens lang zijn.")]
    [Display(Name = "Achternaam")]
    public string Achternaam { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mailadres is verplicht.")]
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres.")]
    [Display(Name = "E-mailadres")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefoonnummer mag maximaal 20 tekens lang zijn.")]
    [Display(Name = "Telefoonnummer")]
    public string Telefoon { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wachtwoord is verplicht.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Wachtwoord moet minstens 6 tekens bevatten.")]
    [Display(Name = "Wachtwoord")]
    public string Wachtwoord { get; set; } = string.Empty;

    [Required(ErrorMessage = "Herhaal het wachtwoord.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Wachtwoord), ErrorMessage = "Wachtwoorden komen niet overeen.")]
    [Display(Name = "Herhaal Wachtwoord")]
    public string HerhaalWachtwoord { get; set; } = string.Empty;

    [Display(Name = "Ik wil een lidmaatschap afsluiten (korting op terreinen)")]
    public bool IsLid { get; set; } = true;

    // Aliassen voor achterwaartse compatibiliteit
    public string Password
    {
        get => Wachtwoord;
        set => Wachtwoord = value;
    }
    public string ConfirmPassword
    {
        get => HerhaalWachtwoord;
        set => HerhaalWachtwoord = value;
    }
    public bool IsMember
    {
        get => IsLid;
        set => IsLid = value;
    }
}
