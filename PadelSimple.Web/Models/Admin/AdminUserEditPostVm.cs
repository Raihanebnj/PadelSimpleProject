using System.ComponentModel.DataAnnotations;

namespace PadelSimple.Web.Models.Admin;

public class AdminUserEditPostVm
{
    [Required]
    public string Id { get; set; } = "";

    public bool IsMember { get; set; }
    public bool IsBlocked { get; set; }

    public List<string>? SelectedRoles { get; set; }
}
