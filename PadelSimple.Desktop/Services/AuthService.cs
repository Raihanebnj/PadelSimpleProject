using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PadelSimple.Models.Identity;

namespace PadelSimple.Desktop.Services;
public class AuthService
{
    private readonly SignInManager<AppUser> _signIn;
    private readonly UserManager<AppUser> _users;
    public AppUser? CurrentUser { get; private set; }
    public IReadOnlyList<string> CurrentRoles { get; private set; } = Array.Empty<string>();

    public AuthService(SignInManager<AppUser> signIn, UserManager<AppUser> users)
    {
        _signIn = signIn; _users = users;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user is null || user.IsDeleted) return false;
        if (!await _users.CheckPasswordAsync(user, password)) return false;
        CurrentUser = user;
        var roles = await _users.GetRolesAsync(user);
        CurrentRoles = roles.ToArray();
        return true;
    }

    public void Logout()
    {
        CurrentUser = null; CurrentRoles = Array.Empty<string>();
    }

    public bool IsInRole(string role) => CurrentRoles.Contains(role);
}