using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Data;

namespace TechStore.ViewComponents;

public class CatalogSummaryViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;
    public CatalogSummaryViewComponent(ApplicationDbContext db) => _db = db;
    public async Task<IViewComponentResult> InvokeAsync()
    {
        ViewBag.ProductCount = await _db.Productos.CountAsync(p => p.Estado && p.Stock > 0);
        ViewBag.CategoryCount = await _db.Categorias.CountAsync();
        return View();
    }
}
