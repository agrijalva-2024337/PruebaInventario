using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SLCDM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AsignacionUbicacionUsoYFirmaRecibe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_ubicacion",
                table: "asignacion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_asignacion_id_ubicacion",
                table: "asignacion",
                column: "id_ubicacion");

            migrationBuilder.AddForeignKey(
                name: "FK_asignacion_ubicacion_id_ubicacion",
                table: "asignacion",
                column: "id_ubicacion",
                principalTable: "ubicacion",
                principalColumn: "id_ubicacion",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_asignacion_ubicacion_id_ubicacion",
                table: "asignacion");

            migrationBuilder.DropIndex(
                name: "IX_asignacion_id_ubicacion",
                table: "asignacion");

            migrationBuilder.DropColumn(
                name: "id_ubicacion",
                table: "asignacion");
        }
    }
}
