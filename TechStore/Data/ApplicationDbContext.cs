using Microsoft.EntityFrameworkCore;
using TechStore.Models;

namespace TechStore.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Descripcion).HasMaxLength(500);
            entity.Property(p => p.Precio).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.Categoria)
                  .WithMany(c => c.Productos)
                  .HasForeignKey(p => p.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Descripcion).HasMaxLength(300);
        });

        // Datos semilla para no partir con la base vacía.
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Computadoras", Descripcion = "Laptops y PCs para el día a día." },
            new Categoria { Id = 2, Nombre = "Celulares", Descripcion = "Smartphones de las mejores marcas." },
            new Categoria { Id = 3, Nombre = "Accesorios", Descripcion = "Mouse, teclados, audífonos y más." },
            new Categoria { Id = 4, Nombre = "Gaming", Descripcion = "Todo lo que necesitás para jugar." }
        );

        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Mackbook Air", Descripcion = "Laptop ligera con pantalla de 14 pulgadas.", Precio = 899.00m, Imagen = "/images/laptop1.png", Stock = 8, Estado = true, CategoriaId = 1 },
            new Producto { Id = 2, Nombre = "Iphone 17", Descripcion = "Smartphone con cámara nítida y batería duradera.", Precio = 649.00m, Imagen = "/images/telefono1.png", Stock = 14, Estado = true, CategoriaId = 2 },
            new Producto { Id = 3, Nombre = "Sony WH-1000XM4", Descripcion = "Audífonos inalámbricos con sonido envolvente.", Precio = 129.00m, Imagen = "/images/audifono1.png", Stock = 20, Estado = true, CategoriaId = 3 },
            new Producto { Id = 4, Nombre = "Ps5 Dualsense", Descripcion = "Control ergonómico para tus sesiones de juego.", Precio = 79.00m, Imagen = "/images/control1.png", Stock = 5, Estado = true, CategoriaId = 4 },
            new Producto { Id = 5, Nombre = "Monitor 4K", Descripcion = "Monitor Full HD de 27 pulgadas y 120 Hz.", Precio = 269.00m, Imagen = "/images/monitor1.png", Stock = 7, Estado = true, CategoriaId = 1 },
            new Producto { Id = 6, Nombre = "Apple Watch", Descripcion = "Reloj inteligente para tu ritmo diario.", Precio = 159.00m, Imagen = "/images/reloj1.png", Stock = 0, Estado = false, CategoriaId = 3 }
        );
    }
}