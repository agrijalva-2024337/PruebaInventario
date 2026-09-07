using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SLCDM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivoPerifericosAdicionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "perifericos_adicionales",
                table: "activo",
                type: "varchar(300)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "perifericos_adicionales",
                table: "activo");
        }
    }
}
