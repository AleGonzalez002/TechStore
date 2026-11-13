using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models;
using TechStore.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
});
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Logging.AddProvider(new TechStore.Logging.FileLoggerProvider(Path.Combine(builder.Environment.ContentRootPath, "logs")));

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

await SeedRolesAndAdministratorAsync(app);
app.Run();

static async Task SeedRolesAndAdministratorAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Bootstrap");
    try
    {
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in new[] { "Administrador", "Vendedor", "Cliente" })
            if (!await roles.RoleExistsAsync(name)) await roles.CreateAsync(new IdentityRole(name));

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var email = config["AdminSeed:Email"];
        var password = config["AdminSeed:Password"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await users.FindByEmailAsync(email);
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, NombreCompleto = "Administrador TechStore" };
                var created = await users.CreateAsync(admin, password);
                if (!created.Succeeded) throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
            }
            if (!await users.IsInRoleAsync(admin, "Administrador")) await users.AddToRoleAsync(admin, "Administrador");
        }
        logger.LogInformation("Roles de TechStore listos. Los registros nuevos reciben el rol Cliente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudieron inicializar roles y usuarios. Verifique la conexión y aplique las migraciones.");
    }
}

public partial class Program { }
