using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyAuthApp.Data;
using MyAuthApp.Models;

namespace MyAuthApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly AppIdentityDbContext _appIdentityDbContext;
    private readonly RoleManager<IdentityRole> _roleManager;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        AppIdentityDbContext appIdentityDbContext,
        RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _appIdentityDbContext = appIdentityDbContext;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        await SeedUsersAndRoles();
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize(Roles = "admin")]
    public IActionResult AdminPage()
    {
        return View();
    }

    [Authorize(Roles = "user")]
    public IActionResult UserPage()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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