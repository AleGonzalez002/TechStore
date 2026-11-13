using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using TechStore.Data;
using TechStore.Models;

namespace TechStore.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReportsController> _logger;
    public ReportsController(ApplicationDbContext db, ILogger<ReportsController> logger) { _db = db; _logger = logger; }

    public IActionResult Index() => View();

    public async Task<IActionResult> Pdf()
    {
        var productos = await _db.Productos.Include(p => p.Categoria).AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var pdf = QuestPDF.Fluent.Document.Create(container => container.Page(page =>
        {
            page.Margin(35);
            page.Header().Column(c => { c.Item().Text("TechStore").FontSize(22).Bold().FontColor("#176B87"); c.Item().Text("Reporte de productos").FontSize(15); c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9); });
            page.Content().PaddingVertical(18).Table(table =>
            {
                table.ColumnsDefinition(columns => { columns.RelativeColumn(1.5f); columns.RelativeColumn(2.4f); columns.RelativeColumn(1); columns.RelativeColumn(1.2f); columns.RelativeColumn(.7f); columns.RelativeColumn(.8f); });
                table.Header(h => { foreach (var title in new[] { "Producto", "Descripción", "Precio", "Categoría", "Stock", "Estado" }) h.Cell().Background("#176B87").Padding(5).Text(title).FontColor("#FFFFFF").Bold().FontSize(8); });
                foreach (var p in productos)
                {
                    table.Cell().BorderBottom(1).BorderColor("#DCE5EA").Padding(5).Text(p.Nombre).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#DCE5EA").Padding(5).Text(p.Descripcion).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#DCE5EA").Padding(5).Text($"${p.Precio:N2}").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#DCE5EA").Padding(5).Text(p.Categoria?.Nombre ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#DCE5EA").Padding(5).Text(p.Stock.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#DCE5EA").Padding(5).Text(p.Estado ? "Activo" : "Inactivo").FontSize(8);
                }
            });
            page.Footer().AlignRight().Text(text => { text.Span("Página "); text.CurrentPageNumber(); });
        })).GeneratePdf();
        _logger.LogInformation("Reporte PDF generado con {Count} productos por {User}", productos.Count, User.Identity?.Name);
        return File(pdf, "application/pdf", $"TechStore-Productos-{DateTime.Now:yyyyMMdd-HHmm}.pdf");
    }

    public async Task<IActionResult> Excel()
    {
        var productos = await _db.Productos.Include(p => p.Categoria).AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var sheet = workbook.Worksheets.Add("Productos");
        var headers = new[] { "ID", "Nombre", "Descripción", "Precio", "Categoría", "Stock", "Estado" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var row = 2;
        foreach (var p in productos)
        {
            sheet.Cell(row, 1).Value = p.Id; sheet.Cell(row, 2).Value = p.Nombre; sheet.Cell(row, 3).Value = p.Descripcion;
            sheet.Cell(row, 4).Value = p.Precio; sheet.Cell(row, 5).Value = p.Categoria?.Nombre ?? "-";
            sheet.Cell(row, 6).Value = p.Stock; sheet.Cell(row, 7).Value = p.Estado ? "Activo" : "Inactivo"; row++;
        }
        var header = sheet.Range(1, 1, 1, headers.Length);
        header.Style.Font.Bold = true; header.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
        header.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#176B87");
        sheet.SheetView.FreezeRows(1); sheet.Columns().AdjustToContents(); sheet.Column(3).Width = Math.Min(sheet.Column(3).Width, 55);
        using var output = new MemoryStream(); workbook.SaveAs(output);
        _logger.LogInformation("Reporte Excel generado con {Count} productos por {User}", productos.Count, User.Identity?.Name);
        return File(output.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TechStore-Productos-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
    }
}
