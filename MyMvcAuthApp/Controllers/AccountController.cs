using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyMvcAuthApp.Data;
using MyMvcAuthApp.Models;
using Vereyon.Web;

namespace MyMvcAuthApp.Controllers;

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
            var sanitizer = new HtmlSanitizer();
            model.UserName = sanitizer.Sanitize(model.UserName);

            var userSignedIn = await _signInManager
                .PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
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
        // 1. Fetch user info
        var userSql = @"SELECT Id, Email, UserName
                        FROM AspNetUsers
                        WHERE Id = {0}";
        
        var user = await _appIdentityDbContext.Users
            .FromSqlRaw(userSql, Id)
            .Select(x => new{ x.Id, x.Email, x.UserName })
            .FirstOrDefaultAsync();
        
        if (user == null)
            return NotFound();
        
        
        // 2. Fetch roles for the user
        var rolesSql = @"SELECT r.Name
                         FROM AspNetUserRoles ur
                         INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                         WHERE ur.UserId = {0}";
        
        var roles = await _appIdentityDbContext.Roles
            .FromSqlRaw(rolesSql, Id)
            .Select(r => r.Name)
            .ToListAsync();
        
        
        // 3. Prepare view model
        var result = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Roles = roles,
        };
        
        ViewBag.AllRoles = roles
            .Select(x => new SelectListItem() { Text = x, Value = x.ToLower() })
            .ToList();
        
        return View(result);
    }

    public async Task<IActionResult> EditUserPost(EditUserViewModel model)
    {
        var selectedUser = await _userManager.FindByIdAsync(model.Id);
        var sanitizer = new HtmlSanitizer();
        
        if (ModelState.IsValid)
        {   
            selectedUser.Email = sanitizer.Sanitize(model.Email);
            selectedUser.UserName = sanitizer.Sanitize(model.UserName);
            var userUpdated = await _userManager.UpdateAsync(selectedUser);
            
            foreach (var (role, index) in model.Roles.Select((item, index) =>  (item, index)))
            {
                model.Roles[index] = sanitizer.Sanitize(role); 
            }
            
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