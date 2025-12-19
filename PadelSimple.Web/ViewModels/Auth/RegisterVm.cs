using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.ViewModels.Auth;

public class RegisterVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";

    public bool IsMember { get; set; } = true;
}
