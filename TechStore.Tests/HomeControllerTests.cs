using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TechStore.Controllers;
using TechStore.Models;
using TechStore.Services;
using Xunit;

namespace TechStore.Tests;

public class HomeControllerTests
{
    [Fact]
    public async Task Index_ConsultaProductosAlServicioYDevuelveVista()
    {
        var products = new Mock<IProductoService>();
        products.Setup(s => s.ObtenerTodosAsync()).ReturnsAsync(new List<Producto>
        {
            new() { Id = 1, Nombre = "Laptop", Estado = true, Stock = 2 },
            new() { Id = 2, Nombre = "Teléfono", Estado = true, Stock = 1 },
            new() { Id = 3, Nombre = "Auriculares", Estado = false, Stock = 0 },
            new() { Id = 4, Nombre = "Monitor", Estado = true, Stock = 4 }
        });
        var categories = new Mock<ICategoriaService>();
        var controller = new HomeController(products.Object, categories.Object, NullLogger<HomeController>.Instance);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Producto>>(view.Model);
        Assert.Equal(new[] { "Laptop", "Teléfono", "Monitor" }, model.Select(p => p.Nombre));
        products.Verify(s => s.ObtenerTodosAsync(), Times.Once);
    }
}
