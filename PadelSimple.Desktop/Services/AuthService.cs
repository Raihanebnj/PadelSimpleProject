using Microsoft.AspNetCore.Identity;
using PadelSimple.Models.Identity;

namespace PadelSimple.Desktop.Services;

public class AuthService
{
    private readonly UserManager<AppUser> _userManager;

    public AppUser? CurrentUser { get; private set; }

    public AuthService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> LoginAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null || user.IsBlocked)
            return false;

        if (await _userManager.CheckPasswordAsync(user, password))
        {
            CurrentUser = user;
            return true;
        }

        return false;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public bool IsInRole(string roleName)
    {
        if (CurrentUser == null) return false;
        return _userManager.IsInRoleAsync(CurrentUser, roleName).Result;
    }
}

