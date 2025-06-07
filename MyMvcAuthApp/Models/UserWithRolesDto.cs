using Microsoft.AspNetCore.Identity;

namespace MyMvcAuthApp.Models;

public class UserWithRolesDto : IdentityUser
{
    public List<string> Roles { get; set; }
}