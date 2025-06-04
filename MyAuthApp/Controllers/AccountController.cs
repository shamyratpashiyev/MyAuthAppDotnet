using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAuthApp.Data;
using MyAuthApp.Models;

namespace MyAuthApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly AppIdentityDbContext _appIdentityDbContext;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        AppIdentityDbContext appIdentityDbContext,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _appIdentityDbContext = appIdentityDbContext;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser()
            {
                UserName = model.Email,
                Email = model.Email,
            };
            var userCreated = await _userManager.CreateAsync(user, model.Password);

            if (userCreated.Succeeded)
            {
                return RedirectToAction("Login");
            }

            foreach (var error in userCreated.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userSignedIn = await _signInManager
                .PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (userSignedIn.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ModelState.AddModelError("", "Invalid login attempt.");
        }
        return View(model);
    }

    [HttpGet("/users-list")]
    public async Task<IActionResult> GetUserList()
    {        
        var userList = await _appIdentityDbContext.Users.ToListAsync();
        var result = new List<UserWithRolesDto>();
        foreach (var user in userList)
        {
            var userDto = new UserWithRolesDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
            };
            
            result.Add(userDto);
        }

        return View("UsersList", result);
    }

    public async Task<IActionResult> EditUserGet(string Id)
    {
        var selectedUser = await _userManager.FindByIdAsync(Id);
        var result = new EditUserViewModel()
        {
            Id = selectedUser.Id,
            Email = selectedUser.Email,
            UserName = selectedUser.UserName,
            Roles = (await _userManager.GetRolesAsync(selectedUser)).ToList(),
        };
        ViewBag.AllRoles = (await _userManager.GetRolesAsync(selectedUser)).ToList()
            .Select(x => new SelectListItem() { Text = x, Value = x.ToLower() }).ToList();
        return View(result);
    }

    public async Task<IActionResult> EditUserPost(EditUserViewModel model)
    {
        var selectedUser = await _userManager.FindByIdAsync(model.Id);

        if (ModelState.IsValid)
        {   
            selectedUser.Email = model.Email;
            selectedUser.UserName = model.UserName;
            var userUpdated = await _userManager.UpdateAsync(selectedUser);
            
            await _userManager.RemoveFromRolesAsync(selectedUser, await _userManager.GetRolesAsync(selectedUser));
            await _userManager.AddToRolesAsync(selectedUser, model.Roles);

            if (userUpdated.Succeeded)
            {
                return RedirectToAction("GetUserList");
            }

            foreach (var error in userUpdated.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        
        return RedirectToAction("EditUserGet", new { Id = model.Id });
    }
}