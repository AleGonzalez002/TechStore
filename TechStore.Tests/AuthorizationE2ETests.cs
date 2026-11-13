using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechStore.Data;
using TechStore.Models;
using Xunit;

namespace TechStore.Tests;

public class AuthorizationE2ETests
{
    private const string AdminEmail = "admin@test.techstore.local";
    private const string AdminPassword = "TestAdmin123!";

    private sealed class TechStoreFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = "e2e-" + Guid.NewGuid();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("AdminSeed:Email", AdminEmail);
            builder.UseSetting("AdminSeed:Password", AdminPassword);
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    [Fact]
    public async Task Cliente_IniciaSesion_VeCatalogoYRecibeDenegacionEnRutasAdministrativas()
    {
        using var factory = new TechStoreFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var register = await GetFormAsync(client, "/Account/Register");
        using var registerResponse = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = register.Token,
            ["NombreCompleto"] = "Cliente Prueba",
            ["Email"] = "cliente@test.techstore.local",
            ["Password"] = "Cliente123!",
            ["ConfirmarPassword"] = "Cliente123!"
        }));
        Assert.Equal(HttpStatusCode.Redirect, registerResponse.StatusCode);
        Assert.Equal("Cliente", (await factory.Services.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync("cliente@test.techstore.local") is { } user
            ? (await factory.Services.GetRequiredService<UserManager<ApplicationUser>>().GetRolesAsync(user)).Single() : ""));

        var home = await client.GetStringAsync("/");
        Assert.Contains("Productos", home);
        Assert.DoesNotContain("Usuarios", home);
        Assert.DoesNotContain("Administrar", home);

        using var protectedResponse = await client.GetAsync("/Productos");
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
        Assert.Contains("/Home/AccessDenied", protectedResponse.Headers.Location!.ToString());
        var denied = await client.GetStringAsync("/Home/AccessDenied");
        Assert.Contains("Acceso denegado", denied);
    }

    [Fact]
    public async Task Administrador_IniciaSesion_YVeAdministracionYReportes()
    {
        using var factory = new TechStoreFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        string assignedUserId;
        using (var scope = factory.Services.CreateScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var account = new ApplicationUser { UserName = "asignado@test.techstore.local", Email = "asignado@test.techstore.local", EmailConfirmed = true, NombreCompleto = "Cuenta Asignada" };
            Assert.True((await manager.CreateAsync(account, "Asignado123!")).Succeeded);
            assignedUserId = account.Id;
        }
        var login = await GetFormAsync(client, "/Account/Login");
        using var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = login.Token,
            ["Email"] = AdminEmail,
            ["Password"] = AdminPassword
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var home = await client.GetStringAsync("/");
        Assert.Contains("Administrar", home);
        Assert.Contains("Reportes", home);
        Assert.Contains("Usuarios", home);
        using var pdf = await client.GetAsync("/Reports/Pdf");
        Assert.Equal(HttpStatusCode.OK, pdf.StatusCode);
        Assert.Equal("application/pdf", pdf.Content.Headers.ContentType!.MediaType);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(await pdf.Content.ReadAsByteArrayAsync()));
        using var excel = await client.GetAsync("/Reports/Excel");
        Assert.Equal(HttpStatusCode.OK, excel.StatusCode);
        Assert.StartsWith("PK", System.Text.Encoding.ASCII.GetString(await excel.Content.ReadAsByteArrayAsync()).Substring(0, 2));
        var usersPage = await GetFormAsync(client, "/Account/Users");
        using var changeRole = await client.PostAsync("/Account/ChangeRole", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = usersPage.Token,
            ["UserId"] = assignedUserId,
            ["Role"] = "Vendedor"
        }));
        Assert.Equal(HttpStatusCode.Redirect, changeRole.StatusCode);
    }

    [Fact]
    public async Task Vendedor_PuedeEditarYGenerarReportesPeroNoEliminar()
    {
        using var factory = new TechStoreFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using (var scope = factory.Services.CreateScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var vendor = new ApplicationUser { UserName = "vendedor@test.techstore.local", Email = "vendedor@test.techstore.local", EmailConfirmed = true, NombreCompleto = "Vendedor Prueba" };
            Assert.True((await manager.CreateAsync(vendor, "Vendedor123!")).Succeeded);
            Assert.True((await manager.AddToRoleAsync(vendor, "Vendedor")).Succeeded);
        }
        var login = await GetFormAsync(client, "/Account/Login");
        using var loginResponse = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = login.Token,
            ["Email"] = "vendedor@test.techstore.local",
            ["Password"] = "Vendedor123!"
        }));
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Productos")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Reports")).StatusCode);
        Assert.DoesNotContain("/Productos/Delete", await client.GetStringAsync("/Productos"));
        using var delete = await client.GetAsync("/Productos/Delete/1");
        Assert.Equal(HttpStatusCode.Redirect, delete.StatusCode);
        Assert.Contains("/Home/AccessDenied", delete.Headers.Location!.ToString());
    }
    private static async Task EnsureCategoriaAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        if (!await db.Categorias.AnyAsync(c => c.Id == 1))
        {
            db.Categorias.Add(new Categoria { Id = 1, Nombre = "Computadoras", Descripcion = "Equipos" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task LoginAsync(HttpClient client, string email, string password)
    {
        var login = await GetFormAsync(client, "/Account/Login");
        using var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = login.Token,
            ["Email"] = email,
            ["Password"] = password
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Vendedor_CreaYEditaProducto_YApareceEnElCatalogo()
    {
        using var factory = new TechStoreFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using (var scope = factory.Services.CreateScope())
        {
            await EnsureCategoriaAsync(scope.ServiceProvider);
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var vendor = new ApplicationUser { UserName = "vendedor2@test.techstore.local", Email = "vendedor2@test.techstore.local", EmailConfirmed = true, NombreCompleto = "Vendedor Productos" };
            Assert.True((await manager.CreateAsync(vendor, "Vendedor123!")).Succeeded);
            Assert.True((await manager.AddToRoleAsync(vendor, "Vendedor")).Succeeded);
        }
        await LoginAsync(client, "vendedor2@test.techstore.local", "Vendedor123!");

        var create = await GetFormAsync(client, "/Productos/Create");
        using var createResponse = await client.PostAsync("/Productos/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = create.Token,
            ["Nombre"] = "Laptop E2E",
            ["Descripcion"] = "Producto creado en la prueba E2E",
            ["Precio"] = "250",
            ["Stock"] = "5",
            ["Imagen"] = "/images/laptop1.png",
            ["Estado"] = "true",
            ["CategoriaId"] = "1"
        }));
        Assert.Equal(HttpStatusCode.Redirect, createResponse.StatusCode);
        Assert.Contains("Laptop E2E", await client.GetStringAsync("/Productos"));

        int id;
        using (var scope = factory.Services.CreateScope())
            id = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Productos.Single(p => p.Nombre == "Laptop E2E").Id;

        var edit = await GetFormAsync(client, $"/Productos/Edit/{id}");
        using var editResponse = await client.PostAsync($"/Productos/Edit/{id}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = edit.Token,
            ["Id"] = id.ToString(),
            ["Nombre"] = "Laptop E2E Editada",
            ["Descripcion"] = "Producto editado en la prueba E2E",
            ["Precio"] = "300",
            ["Stock"] = "4",
            ["Imagen"] = "/images/laptop1.png",
            ["Estado"] = "true",
            ["CategoriaId"] = "1"
        }));
        Assert.Equal(HttpStatusCode.Redirect, editResponse.StatusCode);
        Assert.Contains("Laptop E2E Editada", await client.GetStringAsync("/Productos"));
        Assert.Contains("Laptop E2E Editada", await client.GetStringAsync("/Home/Productos"));
    }

    [Fact]
    public async Task Administrador_EliminaProducto_YDesapareceDelListado()
    {
        using var factory = new TechStoreFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        int id;
        using (var scope = factory.Services.CreateScope())
        {
            await EnsureCategoriaAsync(scope.ServiceProvider);
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var producto = new Producto { Nombre = "Producto Eliminable E2E", Descripcion = "Se elimina en la prueba", Precio = 10m, Stock = 1, Estado = true, Imagen = "/images/laptop1.png", CategoriaId = 1 };
            db.Productos.Add(producto);
            await db.SaveChangesAsync();
            id = producto.Id;
        }
        await LoginAsync(client, AdminEmail, AdminPassword);
        var listado = await client.GetStringAsync("/Productos");
        Assert.Contains("Producto Eliminable E2E", listado);
        Assert.Contains("/Productos/Delete/" + id, listado);

        var delete = await GetFormAsync(client, $"/Productos/Delete/{id}");
        using var deleteResponse = await client.PostAsync($"/Productos/Delete/{id}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = delete.Token,
            ["Id"] = id.ToString()
        }));
        Assert.Equal(HttpStatusCode.Redirect, deleteResponse.StatusCode);
        Assert.DoesNotContain("Producto Eliminable E2E", await client.GetStringAsync("/Productos"));
    }

    private static async Task<(string Html, string Token)> GetFormAsync(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, "No se encontró el token antifalsificación en el formulario.");
        return (html, WebUtility.HtmlDecode(match.Groups[1].Value));
    }
}
