using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TechStore.Data;
using TechStore.Models;
using TechStore.Services;
using Xunit;

namespace TechStore.Tests;

public class ProductoServiceTests
{
    private static (ApplicationDbContext Db, ProductoService Service) CrearServicio()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new ApplicationDbContext(options);
        db.Categorias.Add(new Categoria { Id = 1, Nombre = "Computadoras", Descripcion = "Equipos" });
        db.SaveChanges();
        return (db, new ProductoService(db));
    }

    private static Producto ProductoDemo(string name = "Laptop") => new()
    { Nombre = name, Descripcion = "Equipo de prueba", Precio = 499m, Stock = 5, Estado = true, CategoriaId = 1 };

    [Fact]
    public async Task ObtenerTodos_DevuelveProductosConCategoria()
    {
        var (db, service) = CrearServicio(); using (db) { db.Productos.Add(ProductoDemo()); await db.SaveChangesAsync();
            var items = await service.ObtenerTodosAsync(); Assert.Single(items); Assert.Equal("Computadoras", items[0].Categoria?.Nombre); }
    }

    [Fact]
    public async Task ObtenerPorId_DevuelveProductoSolicitado()
    {
        var (db, service) = CrearServicio(); using (db) { var item = ProductoDemo(); db.Productos.Add(item); await db.SaveChangesAsync();
            var result = await service.ObtenerPorIdAsync(item.Id); Assert.NotNull(result); Assert.Equal(item.Nombre, result!.Nombre); }
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_DevuelveNull()
    { var (db, service) = CrearServicio(); using (db) Assert.Null(await service.ObtenerPorIdAsync(900)); }

    [Fact]
    public async Task Registrar_PersisteProducto()
    { var (db, service) = CrearServicio(); using (db) { await service.AgregarAsync(ProductoDemo()); Assert.Equal(1, await db.Productos.CountAsync()); } }

    [Fact]
    public async Task Editar_ActualizaProductoExistente()
    { var (db, service) = CrearServicio(); using (db) { var item = ProductoDemo(); db.Productos.Add(item); await db.SaveChangesAsync(); item.Nombre = "Laptop nueva";
        Assert.True(await service.EditarAsync(item)); Assert.Equal("Laptop nueva", (await db.Productos.FindAsync(item.Id))!.Nombre); } }

    [Fact]
    public async Task Editar_Inexistente_DevuelveFalse()
    { var (db, service) = CrearServicio(); using (db) Assert.False(await service.EditarAsync(ProductoDemo())); }

    [Fact]
    public async Task Eliminar_QuitaProducto()
    { var (db, service) = CrearServicio(); using (db) { var item = ProductoDemo(); db.Productos.Add(item); await db.SaveChangesAsync();
        Assert.True(await service.EliminarAsync(item.Id)); Assert.Empty(await db.Productos.ToListAsync()); } }

    [Fact]
    public async Task Eliminar_Inexistente_DevuelveFalse()
    { var (db, service) = CrearServicio(); using (db) Assert.False(await service.EliminarAsync(900)); }

    [Fact]
    public async Task Integracion_CrearConsultarEditarYEliminar_ConservaConsistencia()
    {
        var (db, service) = CrearServicio(); using (db)
        {
            var item = ProductoDemo("Monitor"); await service.AgregarAsync(item);
            Assert.True(await service.ExisteAsync(item.Id));
            item.Precio = 299m; Assert.True(await service.EditarAsync(item));
            Assert.Equal(299m, (await service.ObtenerPorIdAsync(item.Id))!.Precio);
            Assert.True(await service.EliminarAsync(item.Id)); Assert.False(await service.ExisteAsync(item.Id));
        }
    }
}
