using Microsoft.AspNetCore.Identity;

namespace TechStore.Models;

public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
}
