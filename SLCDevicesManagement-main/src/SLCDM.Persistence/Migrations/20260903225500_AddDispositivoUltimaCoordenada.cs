using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SLCDM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDispositivoUltimaCoordenada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "origen_coordenada",
                table: "dispositivo_token",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ultimo_bssid",
                table: "dispositivo_token",
                type: "varchar(17)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ultima_latitud",
                table: "dispositivo_token",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ultima_longitud",
                table: "dispositivo_token",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "latitud",
                table: "red_conocida");

            migrationBuilder.DropColumn(
                name: "longitud",
                table: "red_conocida");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "origen_coordenada",
                table: "dispositivo_token");

            migrationBuilder.DropColumn(
                name: "ultimo_bssid",
                table: "dispositivo_token");

            migrationBuilder.DropColumn(
                name: "ultima_latitud",
                table: "dispositivo_token");

            migrationBuilder.DropColumn(
                name: "ultima_longitud",
                table: "dispositivo_token");

            migrationBuilder.AddColumn<decimal>(
                name: "latitud",
                table: "red_conocida",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "longitud",
                table: "red_conocida",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
