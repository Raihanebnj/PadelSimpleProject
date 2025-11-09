using Microsoft.AspNetCore.Identity;
using PadelSimple.Models.Common;


namespace PadelSimple.Models.Identity;


public class AppUser : IdentityUser, ISoftDeletable
{
    public bool IsMember { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
