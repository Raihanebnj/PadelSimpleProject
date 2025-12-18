namespace PadelSimple.Web.Models.Admin;

public class AdminUsersIndexVm
{
    public string Query { get; set; } = "";
    public string RoleFilter { get; set; } = "";
    public bool? BlockedFilter { get; set; }

    public List<string> AvailableRoles { get; set; } = new();
    public List<AdminUserRowVm> Users { get; set; } = new();
}
