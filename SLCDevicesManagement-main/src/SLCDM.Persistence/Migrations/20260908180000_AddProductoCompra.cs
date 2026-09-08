using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SLCDM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductoCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "producto_compra",
                columns: table => new
                {
                    id_producto_compra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_proveedor = table.Column<int>(type: "int", nullable: false),
                    id_categoria = table.Column<int>(type: "int", nullable: true),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    nombre = table.Column<string>(type: "varchar(150)", nullable: false),
                    marca = table.Column<string>(type: "varchar(100)", nullable: true),
                    modelo = table.Column<string>(type: "varchar(100)", nullable: true),
                    descripcion = table.Column<string>(type: "varchar(500)", nullable: true),
                    costo_unitario = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    moneda = table.Column<string>(type: "varchar(10)", nullable: true),
                    numero_factura = table.Column<string>(type: "varchar(50)", nullable: true),
                    fecha_compra = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_vencimiento_garantia = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_compra", x => x.id_producto_compra);
                    table.ForeignKey(
                        name: "FK_producto_compra_proveedor_id_proveedor",
                        column: x => x.id_proveedor,
                        principalTable: "proveedor",
                        principalColumn: "id_proveedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_producto_compra_categoria_activo_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categoria_activo",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_producto_compra_id_proveedor",
                table: "producto_compra",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_producto_compra_id_categoria",
                table: "producto_compra",
                column: "id_categoria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "producto_compra");
        }
    }
}
