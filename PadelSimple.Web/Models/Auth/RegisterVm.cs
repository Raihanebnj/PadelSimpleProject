using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.Models.Auth;

public class RegisterVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    public string PasswordRepeat { get; set; } = "";
    public bool IsMember { get; set; } = true;
}
