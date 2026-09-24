using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace World_Consntrucoes.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;
}