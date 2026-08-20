using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.Models.Auth;

public class RegisterVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6), DataType(DataType.Password)]
    public string Wachtwoord { get; set; } = "";

    [Required, Compare(nameof(Wachtwoord)), DataType(DataType.Password)]
    public string HerhaalWachtwoord { get; set; } = "";
    public bool IsLid { get; set; } = true;
}
