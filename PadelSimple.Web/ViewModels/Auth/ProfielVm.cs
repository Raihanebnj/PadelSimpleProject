using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels.Auth;

public class ProfielVm
{
    [Required(ErrorMessage = "Voornaam is verplicht.")]
    [StringLength(50, ErrorMessage = "Voornaam mag maximaal 50 tekens lang zijn.")]
    [Display(Name = "Voornaam")]
    public string Voornaam { get; set; } = string.Empty;

    [Required(ErrorMessage = "Achternaam is verplicht.")]
    [StringLength(100, ErrorMessage = "Achternaam mag maximaal 100 tekens lang zijn.")]
    [Display(Name = "Achternaam")]
    public string Achternaam { get; set; } = string.Empty;

    [Display(Name = "E-mailadres")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefoonnummer mag maximaal 20 tekens lang zijn.")]
    [Display(Name = "Telefoonnummer")]
    public string Telefoon { get; set; } = string.Empty;

    [Display(Name = "Lidmaatschap actief")]
    public bool IsLid { get; set; }
}
