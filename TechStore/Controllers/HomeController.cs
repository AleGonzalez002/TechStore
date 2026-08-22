using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models;

namespace TechStore.Controllers
{
    public class HomeController : Controller
    {
        // Datos en la memoria.
        private static readonly List<Categoria> CategoriasData =
        [
            new() { Id = 1, Nombre = "Computadoras", Descripcion = "Laptops y PCs para el día a día." },
            new() { Id = 2, Nombre = "Celulares", Descripcion = "Smartphones de las mejores marcas." },
            new() { Id = 3, Nombre = "Accesorios", Descripcion = "Mouse, teclados, audífonos y más." },
            new() { Id = 4, Nombre = "Gaming", Descripcion = "Todo lo que necesitás para jugar." },
        ];

        private static readonly List<Producto> ProductosData =
        [
            new() { Id = 1, Nombre = "Mackbook Air", Descripcion = "Laptop ligera con pantalla de 14 pulgadas.", Precio = 899.00m, Categoria = "Computadoras", Imagen = "/images/laptop1.png", Stock = 8, Estado = true },
            new() { Id = 2, Nombre = "Iphone 17", Descripcion = "Smartphone con cámara nítida y batería duradera.", Precio = 649.00m, Categoria = "Celulares", Imagen = "/images/telefono1.png", Stock = 14, Estado = true },
            new() { Id = 3, Nombre = "Sony WH-1000XM4", Descripcion = "Audífonos inalámbricos con sonido envolvente.", Precio = 129.00m, Categoria = "Accesorios", Imagen = "/images/audifono1.png", Stock = 20, Estado = true },
            new() { Id = 4, Nombre = "Ps5 Dualsense", Descripcion = "Control ergonómico para tus sesiones de juego.", Precio = 79.00m, Categoria = "Gaming", Imagen = "/images/control1.png", Stock = 5, Estado = true },
            new() { Id = 5, Nombre = "Monitor 4K", Descripcion = "Monitor Full HD de 27 pulgadas y 120 Hz.", Precio = 269.00m, Categoria = "Computadoras", Imagen = "/images/monitor1.png", Stock = 7, Estado = true },
            new() { Id = 6, Nombre = "Apple Watch", Descripcion = "Reloj inteligente para tu ritmo diario.", Precio = 159.00m, Categoria = "Accesorios", Imagen = "/images/reloj1.png", Stock = 0, Estado = false }
        ];

        public IActionResult Index()
        {
            return View(ProductosData.Take(3).ToList());
        }

        // Filtro pa las categorias
        public IActionResult Productos(string? categoria)
        {
            var productos = string.IsNullOrWhiteSpace(categoria)
                ? ProductosData
                : ProductosData.Where(p => p.Categoria == categoria).ToList();

            ViewData["CategoriaActual"] = categoria;
            return View(productos);
        }
        public IActionResult Categorias() => View(CategoriasData);
        public IActionResult Contactenos() => View();
        public IActionResult AcercaDe() => View();

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
