using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStore.Models;
using TechStore.Services;

namespace TechStore.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrador,Vendedor")]
    public class ProductosController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IProductoService productoService, ICategoriaService categoriaService, ILogger<ProductosController> logger)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            return View(productos);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCategoriasAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(producto.CategoriaId);
                return View(producto);
            }

            if (await _categoriaService.ObtenerPorIdAsync(producto.CategoriaId) is null)
            {
                ModelState.AddModelError(nameof(producto.CategoriaId), "La categoría seleccionada no existe.");
                await CargarCategoriasAsync(producto.CategoriaId);
                return View(producto);
            }
            await _productoService.AgregarAsync(producto);
            _logger.LogInformation("Producto {ProductoId} creado por {User}", producto.Id, User.Identity?.Name);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto is null)
            {
                return NotFound();
            }

            await CargarCategoriasAsync(producto.CategoriaId);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(producto.CategoriaId);
                return View(producto);
            }

            if (await _categoriaService.ObtenerPorIdAsync(producto.CategoriaId) is null)
            {
                ModelState.AddModelError(nameof(producto.CategoriaId), "La categoría seleccionada no existe.");
                await CargarCategoriasAsync(producto.CategoriaId);
                return View(producto);
            }
            var actualizado = await _productoService.EditarAsync(producto);
            if (!actualizado)
            {
                return NotFound();
            }

            _logger.LogInformation("Producto {ProductoId} actualizado por {User}", producto.Id, User.Identity?.Name);
            return RedirectToAction(nameof(Index));
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto is null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var existe = await _productoService.ExisteAsync(id);
            if (!existe)
            {
                return NotFound();
            }

            await _productoService.EliminarAsync(id);
            _logger.LogInformation("Producto {ProductoId} eliminado por {User}", id, User.Identity?.Name);
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategoriasAsync(int? categoriaSeleccionada = null)
        {
            var categorias = await _categoriaService.ObtenerTodasAsync();
            ViewBag.CategoriaId = new SelectList(categorias, "Id", "Nombre", categoriaSeleccionada);
        }
    }
}
