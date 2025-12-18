using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Identity;
using PadelSimple.Web.Models.Admin;

namespace PadelSimple.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly UserManager<AppUser> _users;
    private readonly RoleManager<AppRole> _roles;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(UserManager<AppUser> users, RoleManager<AppRole> roles, ILogger<AdminUsersController> logger)
    {
        _users = users;
        _roles = roles;
        _logger = logger;
    }

    // /AdminUsers
    [HttpGet]
    public async Task<IActionResult> Index(string? q = null, string? role = null, bool? blocked = null)
    {
        var query = _users.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(q)) ||
                (u.UserName != null && u.UserName.Contains(q)));
        }

        if (blocked.HasValue)
            query = query.Where(u => u.IsBlocked == blocked.Value);

        var list = await query
            .OrderBy(u => u.Email)
            .Take(500)
            .Select(u => new AdminUserRowVm
            {
                Id = u.Id,
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                IsMember = u.IsMember,
                IsBlocked = u.IsBlocked
            })
            .ToListAsync();

        // Roles filter in geheugen (simpel)
        if (!string.IsNullOrWhiteSpace(role))
        {
            var filtered = new List<AdminUserRowVm>();
            foreach (var u in list)
            {
                var dbUser = await _users.FindByIdAsync(u.Id);
                if (dbUser == null) continue;

                var roles = await _users.GetRolesAsync(dbUser);
                if (roles.Contains(role))
                    filtered.Add(u);
            }
            list = filtered;
        }

        var allRoles = await _roles.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync();

        var vm = new AdminUsersIndexVm
        {
            Users = list,
            Query = q ?? "",
            RoleFilter = role ?? "",
            BlockedFilter = blocked,
            AvailableRoles = allRoles
        };

        // Vul rollen per user (voor display)
        foreach (var row in vm.Users)
        {
            var dbUser = await _users.FindByIdAsync(row.Id);
            if (dbUser == null) continue;
            row.Roles = (await _users.GetRolesAsync(dbUser)).OrderBy(x => x).ToList();
        }

        return View(vm);
    }

    // /AdminUsers/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _users.GetRolesAsync(user);
        var allRoles = await _roles.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync();

        var vm = new AdminUserEditVm
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            IsMember = user.IsMember,
            IsBlocked = user.IsBlocked,
            Roles = roles.ToList(),
            AllRoles = allRoles
        };

        return View(vm);
    }

    // Opslaan basisvelden + rollen
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminUserEditPostVm vm)
    {
        if (!ModelState.IsValid)
            return await RebuildEdit(vm);

        var user = await _users.FindByIdAsync(vm.Id);
        if (user == null) return NotFound();

        user.IsMember = vm.IsMember;
        user.IsBlocked = vm.IsBlocked;

        var updateRes = await _users.UpdateAsync(user);
        if (!updateRes.Succeeded)
        {
            foreach (var e in updateRes.Errors) ModelState.AddModelError("", e.Description);
            return await RebuildEdit(vm);
        }

        // Rollen sync
        var currentRoles = await _users.GetRolesAsync(user);
        var selected = (vm.SelectedRoles ?? new List<string>()).Distinct().ToList();

        // Veiligheid: Admin kan zichzelf niet alle Admin-rechten afpakken (optioneel)
        // (laat staan als je dit niet wil)
        if (User.Identity?.Name == user.UserName && !selected.Contains("Admin"))
        {
            ModelState.AddModelError("", "Je kan jezelf niet uit de Admin rol halen.");
            return await RebuildEdit(vm);
        }

        var toRemove = currentRoles.Except(selected).ToList();
        var toAdd = selected.Except(currentRoles).ToList();

        if (toRemove.Any())
            await _users.RemoveFromRolesAsync(user, toRemove);

        if (toAdd.Any())
            await _users.AddToRolesAsync(user, toAdd);

        TempData["Ok"] = "Gebruiker opgeslagen.";
        return RedirectToAction(nameof(Edit), new { id = user.Id });
    }

    // Snelle acties (Index buttons)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsBlocked = true;
        await _users.UpdateAsync(user);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsBlocked = false;
        await _users.UpdateAsync(user);

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> RebuildEdit(AdminUserEditPostVm vm)
    {
        var user = await _users.FindByIdAsync(vm.Id);
        if (user == null) return NotFound();

        var roles = await _users.GetRolesAsync(user);
        var allRoles = await _roles.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync();

        var rebuilt = new AdminUserEditVm
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            IsMember = vm.IsMember,
            IsBlocked = vm.IsBlocked,
            Roles = roles.ToList(),
            AllRoles = allRoles,
            SelectedRoles = vm.SelectedRoles ?? new List<string>()
        };

        return View("Edit", rebuilt);
    }
}
