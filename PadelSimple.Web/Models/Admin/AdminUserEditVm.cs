namespace PadelSimple.Web.Models.Admin;

public class AdminUserEditVm
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";

    public bool IsMember { get; set; }
    public bool IsBlocked { get; set; }

    public List<string> Roles { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();

    // als post faalt
    public List<string> SelectedRoles { get; set; } = new();
}
