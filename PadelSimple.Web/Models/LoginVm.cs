using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.Models;

public class LoginVm
{
    [Required]
    public string EmailOrUserName { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
