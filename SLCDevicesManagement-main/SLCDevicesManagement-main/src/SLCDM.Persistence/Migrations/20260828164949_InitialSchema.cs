using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SLCDM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoria_activo",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria_activo", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    id_empresa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    nit_codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    direccion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    telefono = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresa", x => x.id_empresa);
                });

            migrationBuilder.CreateTable(
                name: "estado",
                columns: table => new
                {
                    id_estado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estado", x => x.id_estado);
                });

            migrationBuilder.CreateTable(
                name: "pais",
                columns: table => new
                {
                    id_pais = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    codigo_iso2 = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    codigo_iso3 = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    codigo_telefonico = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pais", x => x.id_pais);
                });

            migrationBuilder.CreateTable(
                name: "tipo_asignacion",
                columns: table => new
                {
                    id_tipo_asignacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_asignacion", x => x.id_tipo_asignacion);
                });

            migrationBuilder.CreateTable(
                name: "proveedor",
                columns: table => new
                {
                    id_proveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    nit = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    nombre_contacto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    correo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedor", x => x.id_proveedor);
                    table.ForeignKey(
                        name: "FK_proveedor_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresa",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: true),
                    nombres = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    correo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "varchar(50)", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuario_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresa",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sede",
                columns: table => new
                {
                    id_sede = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    id_pais = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    direccion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ciudad = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sede", x => x.id_sede);
                    table.ForeignKey(
                        name: "FK_sede_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresa",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sede_pais_id_pais",
                        column: x => x.id_pais,
                        principalTable: "pais",
                        principalColumn: "id_pais",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bitacora",
                columns: table => new
                {
                    id_bitacora = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tipo_operacion = table.Column<string>(type: "varchar(30)", nullable: false),
                    entidad_afectada = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    informacion_anterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    informacion_nueva = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bitacora", x => x.id_bitacora);
                    table.ForeignKey(
                        name: "FK_bitacora_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "area",
                columns: table => new
                {
                    id_area = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sede = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_area", x => x.id_area);
                    table.ForeignKey(
                        name: "FK_area_sede_id_sede",
                        column: x => x.id_sede,
                        principalTable: "sede",
                        principalColumn: "id_sede",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historico_inventario",
                columns: table => new
                {
                    id_inventario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sede = table.Column<int>(type: "int", nullable: false),
                    cerrado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    responsable = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    fecha_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    observaciones = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historico_inventario", x => x.id_inventario);
                    table.ForeignKey(
                        name: "FK_historico_inventario_sede_id_sede",
                        column: x => x.id_sede,
                        principalTable: "sede",
                        principalColumn: "id_sede",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ubicacion",
                columns: table => new
                {
                    id_ubicacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_sede = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    latitud = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    longitud = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ubicacion", x => x.id_ubicacion);
                    table.ForeignKey(
                        name: "FK_ubicacion_sede_id_sede",
                        column: x => x.id_sede,
                        principalTable: "sede",
                        principalColumn: "id_sede",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "responsable",
                columns: table => new
                {
                    id_responsable = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_area = table.Column<int>(type: "int", nullable: false),
                    nombre_completo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    cargo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    correo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    telefono = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    habilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_responsable", x => x.id_responsable);
                    table.ForeignKey(
                        name: "FK_responsable_area_id_area",
                        column: x => x.id_area,
                        principalTable: "area",
                        principalColumn: "id_area",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activo",
                columns: table => new
                {
                    id_activo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_categoria = table.Column<int>(type: "int", nullable: false),
                    id_proveedor = table.Column<int>(type: "int", nullable: false),
                    id_ubicacion = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    marca = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    modelo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    numero_serie = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    fecha_compra = table.Column<DateTime>(type: "date", nullable: false),
                    costo_adquisicion = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    moneda = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    numero_factura = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    fecha_vencimiento_garantia = table.Column<DateTime>(type: "date", nullable: false),
                    observaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activo", x => x.id_activo);
                    table.ForeignKey(
                        name: "FK_activo_categoria_activo_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categoria_activo",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activo_proveedor_id_proveedor",
                        column: x => x.id_proveedor,
                        principalTable: "proveedor",
                        principalColumn: "id_proveedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activo_ubicacion_id_ubicacion",
                        column: x => x.id_ubicacion,
                        principalTable: "ubicacion",
                        principalColumn: "id_ubicacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asignacion",
                columns: table => new
                {
                    id_asignacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_activo = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    id_responsable = table.Column<int>(type: "int", nullable: false),
                    id_estado = table.Column<int>(type: "int", nullable: false),
                    id_tipo_asignacion = table.Column<int>(type: "int", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_devolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    observaciones = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    firma_entrega = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    fecha_firma_entrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    firma_recibe = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    documento_pdf_url = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    documento_pdf_generado_en = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignacion", x => x.id_asignacion);
                    table.ForeignKey(
                        name: "FK_asignacion_activo_id_activo",
                        column: x => x.id_activo,
                        principalTable: "activo",
                        principalColumn: "id_activo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignacion_estado_id_estado",
                        column: x => x.id_estado,
                        principalTable: "estado",
                        principalColumn: "id_estado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignacion_responsable_id_responsable",
                        column: x => x.id_responsable,
                        principalTable: "responsable",
                        principalColumn: "id_responsable",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignacion_tipo_asignacion_id_tipo_asignacion",
                        column: x => x.id_tipo_asignacion,
                        principalTable: "tipo_asignacion",
                        principalColumn: "id_tipo_asignacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignacion_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalle_activo",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_activo = table.Column<int>(type: "int", nullable: false),
                    id_inventario = table.Column<int>(type: "int", nullable: false),
                    encontrado = table.Column<bool>(type: "bit", nullable: false),
                    buen_estado = table.Column<bool>(type: "bit", nullable: false),
                    observaciones = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    fecha_verificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_activo", x => x.id_detalle);
                    table.ForeignKey(
                        name: "FK_detalle_activo_activo_id_activo",
                        column: x => x.id_activo,
                        principalTable: "activo",
                        principalColumn: "id_activo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalle_activo_historico_inventario_id_inventario",
                        column: x => x.id_inventario,
                        principalTable: "historico_inventario",
                        principalColumn: "id_inventario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historial_activo",
                columns: table => new
                {
                    id_historial_activo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_asignacion = table.Column<int>(type: "int", nullable: true),
                    id_detalle = table.Column<int>(type: "int", nullable: true),
                    fecha_hora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tipo_operacion = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    informacion_anterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    informacion_nueva = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_activo", x => x.id_historial_activo);
                    table.ForeignKey(
                        name: "FK_historial_activo_asignacion_id_asignacion",
                        column: x => x.id_asignacion,
                        principalTable: "asignacion",
                        principalColumn: "id_asignacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_historial_activo_detalle_activo_id_detalle",
                        column: x => x.id_detalle,
                        principalTable: "detalle_activo",
                        principalColumn: "id_detalle",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activo_id_categoria",
                table: "activo",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_activo_id_proveedor",
                table: "activo",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_activo_id_ubicacion",
                table: "activo",
                column: "id_ubicacion");

            migrationBuilder.CreateIndex(
                name: "IX_activo_numero_serie",
                table: "activo",
                column: "numero_serie");

            migrationBuilder.CreateIndex(
                name: "IX_area_id_sede",
                table: "area",
                column: "id_sede");

            migrationBuilder.CreateIndex(
                name: "IX_asignacion_activa",
                table: "asignacion",
                column: "activa");

            migrationBuilder.CreateIndex(
                name: "IX_asignacion_id_activo",
                table: "asignacion",
                column: "id_activo");

            migrationBuilder.CreateIndex(
                name: "IX_asignacion_id_estado",
                table: "asignacion",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_asignacion_id_responsable",
                table: "asignacion",
                column: "id_responsable");

            migrationBuilder.CreateIndex(
                name: "IX_asignacion_id_tipo_asignacion",
                table: "asignacion",
                column: "id_tipo_asignacion");

            migrationBuilder.CreateIndex(
                name: "IX_asignacion_id_usuario",
                table: "asignacion",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_bitacora_fecha_hora",
                table: "bitacora",
                column: "fecha_hora");

            migrationBuilder.CreateIndex(
                name: "IX_bitacora_id_usuario",
                table: "bitacora",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_activo_nombre",
                table: "categoria_activo",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_activo_id_activo",
                table: "detalle_activo",
                column: "id_activo");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_activo_id_inventario",
                table: "detalle_activo",
                column: "id_inventario");

            migrationBuilder.CreateIndex(
                name: "IX_empresa_nit_codigo",
                table: "empresa",
                column: "nit_codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estado_nombre",
                table: "estado",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_historial_activo_fecha_hora",
                table: "historial_activo",
                column: "fecha_hora");

            migrationBuilder.CreateIndex(
                name: "IX_historial_activo_id_asignacion",
                table: "historial_activo",
                column: "id_asignacion");

            migrationBuilder.CreateIndex(
                name: "IX_historial_activo_id_detalle",
                table: "historial_activo",
                column: "id_detalle");

            migrationBuilder.CreateIndex(
                name: "IX_historico_inventario_fecha_inicio",
                table: "historico_inventario",
                column: "fecha_inicio");

            migrationBuilder.CreateIndex(
                name: "IX_historico_inventario_id_sede",
                table: "historico_inventario",
                column: "id_sede");

            migrationBuilder.CreateIndex(
                name: "IX_pais_codigo_iso2",
                table: "pais",
                column: "codigo_iso2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pais_codigo_iso3",
                table: "pais",
                column: "codigo_iso3",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proveedor_id_empresa",
                table: "proveedor",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_proveedor_nit",
                table: "proveedor",
                column: "nit");

            migrationBuilder.CreateIndex(
                name: "IX_responsable_id_area",
                table: "responsable",
                column: "id_area");

            migrationBuilder.CreateIndex(
                name: "IX_sede_id_empresa",
                table: "sede",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_sede_id_pais",
                table: "sede",
                column: "id_pais");

            migrationBuilder.CreateIndex(
                name: "IX_tipo_asignacion_nombre",
                table: "tipo_asignacion",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ubicacion_id_sede",
                table: "ubicacion",
                column: "id_sede");

            migrationBuilder.CreateIndex(
                name: "IX_ubicacion_nombre",
                table: "ubicacion",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_correo",
                table: "usuario",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_empresa",
                table: "usuario",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_username",
                table: "usuario",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bitacora");

            migrationBuilder.DropTable(
                name: "historial_activo");

            migrationBuilder.DropTable(
                name: "asignacion");

            migrationBuilder.DropTable(
                name: "detalle_activo");

            migrationBuilder.DropTable(
                name: "estado");

            migrationBuilder.DropTable(
                name: "responsable");

            migrationBuilder.DropTable(
                name: "tipo_asignacion");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "activo");

            migrationBuilder.DropTable(
                name: "historico_inventario");

            migrationBuilder.DropTable(
                name: "area");

            migrationBuilder.DropTable(
                name: "categoria_activo");

            migrationBuilder.DropTable(
                name: "proveedor");

            migrationBuilder.DropTable(
                name: "ubicacion");

            migrationBuilder.DropTable(
                name: "sede");

            migrationBuilder.DropTable(
                name: "empresa");

            migrationBuilder.DropTable(
                name: "pais");
        }
    }
}
