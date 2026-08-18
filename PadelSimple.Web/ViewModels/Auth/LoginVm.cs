using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels.Auth;

public class LoginVm
{
    [Required(ErrorMessage = "E-mailadres is verplicht.")]
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres.")]
    [Display(Name = "E-mailadres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wachtwoord is verplicht.")]
    [DataType(DataType.Password)]
    [Display(Name = "Wachtwoord")]
    public string Wachtwoord { get; set; } = string.Empty;

    [Display(Name = "Mijn gegevens onthouden")]
    public bool OnthoudMij { get; set; }

    // Aliassen voor compatibiliteit
    public string Password
    {
        get => Wachtwoord;
        set => Wachtwoord = value;
    }
    public bool RememberMe
    {
        get => OnthoudMij;
        set => OnthoudMij = value;
    }
}
