using TechStore.Models;

namespace TechStore.Services;

public interface ICategoriaService
{
    Task<List<Categoria>> ObtenerTodasAsync();
    Task<Categoria?> ObtenerPorIdAsync(int id);
}
