using System.ComponentModel.DataAnnotations;

namespace TechStore.Models;

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    public bool Recordarme { get; set; }
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(120)] public string NombreCompleto { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Confirmar contraseña")] public string ConfirmarPassword { get; set; } = string.Empty;
}

public class UserRoleViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class ChangeRoleViewModel
{
    [Required] public string UserId { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = string.Empty;
}
