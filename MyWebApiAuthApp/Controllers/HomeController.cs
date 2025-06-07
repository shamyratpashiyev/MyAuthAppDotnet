using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApiAuthApp.Services;

namespace MyWebApiAuthApp.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HomeController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtService _jwtService;
    
    public HomeController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return Ok(new { Message = "Hello, Welcome to my API!" });
    }

    [HttpGet]
    public async Task<IActionResult> RegisteredUserListGet()
    {
        await SeedUsersAndRoles();
        var users = await _userManager.Users.Select(x => new { User = x, Roles = new List<string>()}).ToListAsync();
        foreach (var user in users)
        {
            user.Roles.AddRange(await _userManager.GetRolesAsync(user.User));
        }

        return Ok(users);
    }

    [HttpPost]
    public IActionResult GenerateJwtToken(string userName, List<string> roles)
    {
        var token = _jwtService.Create(userName, roles);
        return Ok(new { Token = token });
    }

    [HttpGet]
    public object DecodeToken(string token)
    {
        return _jwtService.Decode(token);
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public IActionResult AdminPage()
    {
        return Ok(new { message = "Welcome to admin page" });
    }
    
    [HttpGet]
    [Authorize(Roles = "user")]
    public IActionResult UserPage()
    {
        return Ok(new { message = "Welcome to user page" });
    }
    
    [HttpGet]
    [Authorize]
    public IActionResult ProtectedPage()
    {
        return Ok(new { message = "Welcome to Protected page" });
    }
    private async Task SeedUsersAndRoles()
    {
        if (await _userManager.FindByNameAsync("testUser") == null)
        {
            await _roleManager.CreateAsync(new()
            {
                Name = "admin",
                NormalizedName = "Admin"
            });
            await _roleManager.CreateAsync(new()
            {
                Name = "user",
                NormalizedName = "User",
            });

            var testUser = new IdentityUser()
            {
                UserName = "testUser",
                Email = "test@email.com"
            };
            await _userManager.CreateAsync(testUser, "PASSword123$%");
            // await _userManager.AddToRoleAsync(testUser, "admin");
            await _userManager.AddToRoleAsync(testUser, "user");
        }
    }
}