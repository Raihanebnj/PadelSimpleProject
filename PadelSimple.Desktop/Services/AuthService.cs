using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Identity;

namespace PadelSimple.Desktop.Services;

public class AuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public AppUser? CurrentUser { get; private set; }
    public IList<string> CurrentRoles { get; private set; } = new List<string>();

    public bool IsAdmin => CurrentRoles.Contains("Admin");
    public bool IsStaff => CurrentRoles.Contains("Staff");
    public bool IsMember => CurrentRoles.Contains("Member");

    public AuthService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ✅ AANGEPASTE LoginAsync
    public async Task<(bool Succeeded, string Error)> LoginAsync(string userNameOrEmail, string password)
    {
        // Eerst zoeken op username
        var user = await _userManager.FindByNameAsync(userNameOrEmail);

        // Als niets gevonden: zoeken op e-mail
        if (user == null)
            user = await _userManager.FindByEmailAsync(userNameOrEmail);

        CurrentUser = user;

        if (CurrentUser == null)
            return (false, "Gebruiker niet gevonden.");

        if (CurrentUser.IsBlocked)
            return (false, "Deze gebruiker is geblokkeerd.");

        var ok = await _userManager.CheckPasswordAsync(CurrentUser, password);
        if (!ok)
        {
            CurrentUser = null;
            CurrentRoles = new List<string>();
            return (false, "Ongeldig wachtwoord.");
        }

        CurrentRoles = await _userManager.GetRolesAsync(CurrentUser);
        return (true, string.Empty);
    }

    public void Logout()
    {
        CurrentUser = null;
        CurrentRoles = new List<string>();
    }

    // Registratie van gewone gebruiker (Member)
    public async Task<(bool Succeeded, string Error)> RegisterAsync(string email, string password)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
            return (false, "Er bestaat al een account met dit e-mailadres.");

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            IsMember = true,
            IsBlocked = false
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var msg = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            return (false, msg);
        }

        await _userManager.AddToRoleAsync(user, "Member");
        return (true, string.Empty);
    }

    // === Admin / user beheer ===

    public async Task<List<AppUser>> GetAllUsersAsync()
    {
        return await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();
    }

    public async Task<IList<string>> GetUserRolesAsync(AppUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task AddRoleAsync(AppUser user, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            throw new InvalidOperationException($"Rol '{roleName}' bestaat niet.");

        if (!await _userManager.IsInRoleAsync(user, roleName))
            await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task RemoveRoleAsync(AppUser user, string roleName)
    {
        if (await _userManager.IsInRoleAsync(user, roleName))
            await _userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task SetBlockedAsync(AppUser user, bool blocked)
    {
        user.IsBlocked = blocked;
        await _userManager.UpdateAsync(user);
    }
}
