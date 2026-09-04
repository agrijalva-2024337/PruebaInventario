using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SLCDM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDispositivoTokenYRedConocida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dispositivo_token",
                columns: table => new
                {
                    id_dispositivo_token = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_activo = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expira_en = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ultimo_uso_en = table.Column<DateTime>(type: "datetime2", nullable: true),
                    revocado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ultima_ubicacion_detectada_id = table.Column<int>(type: "int", nullable: true),
                    fuera_de_rango = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispositivo_token", x => x.id_dispositivo_token);
                    table.ForeignKey(
                        name: "FK_dispositivo_token_activo_id_activo",
                        column: x => x.id_activo,
                        principalTable: "activo",
                        principalColumn: "id_activo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dispositivo_token_ubicacion_ultima_ubicacion_detectada_id",
                        column: x => x.ultima_ubicacion_detectada_id,
                        principalTable: "ubicacion",
                        principalColumn: "id_ubicacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "red_conocida",
                columns: table => new
                {
                    id_red_conocida = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bssid = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: false),
                    id_ubicacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_red_conocida", x => x.id_red_conocida);
                    table.ForeignKey(
                        name: "FK_red_conocida_ubicacion_id_ubicacion",
                        column: x => x.id_ubicacion,
                        principalTable: "ubicacion",
                        principalColumn: "id_ubicacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dispositivo_token_id_activo",
                table: "dispositivo_token",
                column: "id_activo",
                unique: true,
                filter: "[revocado] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_dispositivo_token_token_hash",
                table: "dispositivo_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dispositivo_token_ultima_ubicacion_detectada_id",
                table: "dispositivo_token",
                column: "ultima_ubicacion_detectada_id");

            migrationBuilder.CreateIndex(
                name: "IX_red_conocida_bssid",
                table: "red_conocida",
                column: "bssid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_red_conocida_id_ubicacion",
                table: "red_conocida",
                column: "id_ubicacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispositivo_token");

            migrationBuilder.DropTable(
                name: "red_conocida");
        }
    }
}
