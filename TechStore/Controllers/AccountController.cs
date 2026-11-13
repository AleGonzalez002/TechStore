using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models;

namespace TechStore.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, ILogger<AccountController> logger)
    { _signInManager = signInManager; _userManager = userManager; _roleManager = roleManager; _logger = logger; }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.Recordarme, lockoutOnFailure: true);
        if (result.Succeeded) { _logger.LogInformation("Inicio de sesión correcto para {Email}", model.Email); return LocalRedirect(model.ReturnUrl ?? Url.Action("Index", "Home")!); }
        _logger.LogWarning("Intento de inicio de sesión fallido para {Email}", model.Email);
        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(model);
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, NombreCompleto = model.NombreCompleto, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            if (await _roleManager.RoleExistsAsync("Cliente")) await _userManager.AddToRoleAsync(user, "Cliente");
            _logger.LogInformation("Cuenta de cliente creada para {Email}", model.Email);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var email = User.Identity?.Name;
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Cierre de sesión para {Email}", email);
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.ToList();
        var rows = new List<UserRoleViewModel>();
        foreach (var user in users)
            rows.Add(new UserRoleViewModel { Id = user.Id, Email = user.Email ?? "", NombreCompleto = user.NombreCompleto, Rol = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Sin rol" });
        return View(rows);
    }

    [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
    {
        var allowed = new[] { "Administrador", "Vendedor", "Cliente" };
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null || !allowed.Contains(model.Role)) return NotFound();
        var existing = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, existing);
        if (result.Succeeded) result = await _userManager.AddToRoleAsync(user, model.Role);
        if (!result.Succeeded) { _logger.LogError("No se pudo cambiar el rol de {Email}", user.Email); return BadRequest(result.Errors); }
        _logger.LogInformation("Rol de {Email} actualizado a {Role}", user.Email, model.Role);
        return RedirectToAction(nameof(Users));
    }
}
