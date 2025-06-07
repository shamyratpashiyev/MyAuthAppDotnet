using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyMvcAuthApp.Models;

public class EditUserViewModel
{
    [Required]
    public string Id { get; set; }
    
    [Required]
    public string UserName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public List<string> Roles { get; set; }
}