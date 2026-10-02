using Microsoft.EntityFrameworkCore;
using TechStore.Data;
using TechStore.Models;

namespace TechStore.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> ObtenerTodosAsync()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Producto>> ObtenerPorCategoriaAsync(int categoriaId)
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.CategoriaId == categoriaId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AgregarAsync(Producto producto)
    {
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EditarAsync(Producto producto)
    {
        var existente = await _context.Productos.FirstOrDefaultAsync(p => p.Id == producto.Id);
        if (existente is null)
        {
            return false;
        }

        existente.Nombre = producto.Nombre;
        existente.Descripcion = producto.Descripcion;
        existente.Precio = producto.Precio;
        existente.Imagen = producto.Imagen;
        existente.Stock = producto.Stock;
        existente.Estado = producto.Estado;
        existente.CategoriaId = producto.CategoriaId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
        {
            return false;
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExisteAsync(int id)
    {
        return await _context.Productos.AnyAsync(p => p.Id == id);
    }
}
