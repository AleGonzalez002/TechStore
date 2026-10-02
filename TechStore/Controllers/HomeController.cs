using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models;
using TechStore.Services;

namespace TechStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;

        public HomeController(IProductoService productoService, ICategoriaService categoriaService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            return View(productos.Take(3).ToList());
        }

        // Filtro pa las categorias
        public async Task<IActionResult> Productos(int? categoriaId)
        {
            List<Producto> productos;
            if (categoriaId.HasValue)
            {
                productos = await _productoService.ObtenerPorCategoriaAsync(categoriaId.Value);
                var categoria = await _categoriaService.ObtenerPorIdAsync(categoriaId.Value);
                ViewData["CategoriaActual"] = categoria?.Nombre;
            }
            else
            {
                productos = await _productoService.ObtenerTodosAsync();
                ViewData["CategoriaActual"] = null;
            }

            return View(productos);
        }

        public async Task<IActionResult> Categorias()
        {
            var categorias = await _categoriaService.ObtenerTodasAsync();
            return View(categorias);
        }

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
