using Microsoft.AspNetCore.Identity;

namespace MyAuthApp.Models;

public class UserWithRolesDto : IdentityUser
{
    public List<string> Roles { get; set; }
}