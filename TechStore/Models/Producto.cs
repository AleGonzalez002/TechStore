using System.ComponentModel.DataAnnotations;

namespace TechStore.Models;

public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    public string Imagen { get; set; } = string.Empty;

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    public bool Estado { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una categoría.")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }
}