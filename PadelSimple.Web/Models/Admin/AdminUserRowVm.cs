namespace PadelSimple.Web.Models.Admin;

public class AdminUserRowVm
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public bool IsMember { get; set; }
    public bool IsBlocked { get; set; }
    public List<string> Roles { get; set; } = new();
}
