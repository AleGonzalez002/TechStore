using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models;
using TechStore.Services;

namespace TechStore.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IProductoService _productoService;
    private readonly ICategoriaService _categoriaService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IProductoService productoService, ICategoriaService categoriaService, ILogger<HomeController> logger)
    { _productoService = productoService; _categoriaService = categoriaService; _logger = logger; }

    public async Task<IActionResult> Index()
    {
        try { return View((await _productoService.ObtenerTodosAsync()).Where(p => p.Estado && p.Stock > 0).Take(3).ToList()); }
        catch (Exception ex) { _logger.LogError(ex, "Error al cargar la página inicial"); throw; }
    }

    public async Task<IActionResult> Productos(int? categoriaId)
    {
        try
        {
            List<Producto> productos;
            if (categoriaId.HasValue)
            {
                productos = await _productoService.ObtenerPorCategoriaAsync(categoriaId.Value);
                var categoria = await _categoriaService.ObtenerPorIdAsync(categoriaId.Value);
                ViewData["CategoriaActual"] = categoria?.Nombre;
            }
            else { productos = await _productoService.ObtenerTodosAsync(); ViewData["CategoriaActual"] = null; }
            return View(productos.Where(p => p.Estado && p.Stock > 0).ToList());
        }
        catch (Exception ex) { _logger.LogError(ex, "Error al consultar productos"); throw; }
    }

    public async Task<IActionResult> Categorias()
    {
        try { return View(await _categoriaService.ObtenerTodasAsync()); }
        catch (Exception ex) { _logger.LogError(ex, "Error al consultar categorías"); throw; }
    }

    public IActionResult Contactenos() => View();
    public IActionResult AcercaDe() => View();
    [AllowAnonymous] public IActionResult AccessDenied() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
