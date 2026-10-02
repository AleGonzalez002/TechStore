using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechStore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Laptops y PCs para el día a día.", "Computadoras" },
                    { 2, "Smartphones de las mejores marcas.", "Celulares" },
                    { 3, "Mouse, teclados, audífonos y más.", "Accesorios" },
                    { 4, "Todo lo que necesitás para jugar.", "Gaming" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "CategoriaId", "Descripcion", "Estado", "Imagen", "Nombre", "Precio", "Stock" },
                values: new object[,]
                {
                    { 1, 1, "Laptop ligera con pantalla de 14 pulgadas.", true, "/images/laptop1.png", "Mackbook Air", 899.00m, 8 },
                    { 2, 2, "Smartphone con cámara nítida y batería duradera.", true, "/images/telefono1.png", "Iphone 17", 649.00m, 14 },
                    { 3, 3, "Audífonos inalámbricos con sonido envolvente.", true, "/images/audifono1.png", "Sony WH-1000XM4", 129.00m, 20 },
                    { 4, 4, "Control ergonómico para tus sesiones de juego.", true, "/images/control1.png", "Ps5 Dualsense", 79.00m, 5 },
                    { 5, 1, "Monitor Full HD de 27 pulgadas y 120 Hz.", true, "/images/monitor1.png", "Monitor 4K", 269.00m, 7 },
                    { 6, 3, "Reloj inteligente para tu ritmo diario.", false, "/images/reloj1.png", "Apple Watch", 159.00m, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
