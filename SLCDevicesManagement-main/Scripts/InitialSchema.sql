IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [categoria_activo] (
        [id_categoria] int NOT NULL IDENTITY,
        [nombre] varchar(100) NOT NULL,
        [descripcion] varchar(200) NULL,
        [habilitado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_categoria_activo] PRIMARY KEY ([id_categoria])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [empresa] (
        [id_empresa] int NOT NULL IDENTITY,
        [nombre] varchar(150) NOT NULL,
        [nit_codigo] varchar(50) NOT NULL,
        [direccion] varchar(150) NULL,
        [telefono] varchar(30) NULL,
        [habilitado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_empresa] PRIMARY KEY ([id_empresa])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [estado] (
        [id_estado] int NOT NULL IDENTITY,
        [nombre] varchar(50) NOT NULL,
        [descripcion] varchar(150) NULL,
        CONSTRAINT [PK_estado] PRIMARY KEY ([id_estado])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [pais] (
        [id_pais] int NOT NULL IDENTITY,
        [nombre] varchar(100) NOT NULL,
        [codigo_iso2] varchar(2) NOT NULL,
        [codigo_iso3] varchar(3) NOT NULL,
        [codigo_telefonico] varchar(5) NULL,
        CONSTRAINT [PK_pais] PRIMARY KEY ([id_pais])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [tipo_asignacion] (
        [id_tipo_asignacion] int NOT NULL IDENTITY,
        [nombre] varchar(50) NOT NULL,
        [descripcion] varchar(150) NULL,
        CONSTRAINT [PK_tipo_asignacion] PRIMARY KEY ([id_tipo_asignacion])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [proveedor] (
        [id_proveedor] int NOT NULL IDENTITY,
        [id_empresa] int NOT NULL,
        [nombre] varchar(150) NOT NULL,
        [nit] varchar(50) NOT NULL,
        [nombre_contacto] varchar(100) NULL,
        [telefono] varchar(30) NULL,
        [correo] varchar(150) NULL,
        [habilitado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_proveedor] PRIMARY KEY ([id_proveedor]),
        CONSTRAINT [FK_proveedor_empresa_id_empresa] FOREIGN KEY ([id_empresa]) REFERENCES [empresa] ([id_empresa]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [usuario] (
        [id_usuario] int NOT NULL IDENTITY,
        [id_empresa] int NULL,
        [nombres] varchar(100) NOT NULL,
        [apellidos] varchar(100) NOT NULL,
        [correo] varchar(150) NOT NULL,
        [username] varchar(50) NOT NULL,
        [password_hash] varchar(255) NOT NULL,
        [rol] varchar(50) NOT NULL,
        [fecha_creacion] datetime2 NOT NULL DEFAULT (GETDATE()),
        [habilitado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_usuario] PRIMARY KEY ([id_usuario]),
        CONSTRAINT [FK_usuario_empresa_id_empresa] FOREIGN KEY ([id_empresa]) REFERENCES [empresa] ([id_empresa]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [sede] (
        [id_sede] int NOT NULL IDENTITY,
        [id_empresa] int NOT NULL,
        [id_pais] int NOT NULL,
        [nombre] varchar(100) NOT NULL,
        [direccion] varchar(100) NULL,
        [ciudad] varchar(100) NULL,
        [habilitado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_sede] PRIMARY KEY ([id_sede]),
        CONSTRAINT [FK_sede_empresa_id_empresa] FOREIGN KEY ([id_empresa]) REFERENCES [empresa] ([id_empresa]) ON DELETE NO ACTION,
        CONSTRAINT [FK_sede_pais_id_pais] FOREIGN KEY ([id_pais]) REFERENCES [pais] ([id_pais]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [bitacora] (
        [id_bitacora] int NOT NULL IDENTITY,
        [id_usuario] int NOT NULL,
        [fecha_hora] datetime2 NOT NULL,
        [tipo_operacion] varchar(30) NOT NULL,
        [entidad_afectada] varchar(100) NOT NULL,
        [descripcion] varchar(300) NULL,
        [informacion_anterior] nvarchar(max) NULL,
        [informacion_nueva] nvarchar(max) NULL,
        CONSTRAINT [PK_bitacora] PRIMARY KEY ([id_bitacora]),
        CONSTRAINT [FK_bitacora_usuario_id_usuario] FOREIGN KEY ([id_usuario]) REFERENCES [usuario] ([id_usuario]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [area] (
        [id_area] int NOT NULL IDENTITY,
        [id_sede] int NOT NULL,
        [nombre] varchar(100) NOT NULL,
        [descripcion] varchar(200) NULL,
        [habilitado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_area] PRIMARY KEY ([id_area]),
        CONSTRAINT [FK_area_sede_id_sede] FOREIGN KEY ([id_sede]) REFERENCES [sede] ([id_sede]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [historico_inventario] (
        [id_inventario] int NOT NULL IDENTITY,
        [id_sede] int NOT NULL,
        [cerrado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [responsable] varchar(150) NULL,
        [fecha_inicio] datetime2 NOT NULL,
        [fecha_cierre] datetime2 NULL,
        [observaciones] varchar(300) NULL,
        CONSTRAINT [PK_historico_inventario] PRIMARY KEY ([id_inventario]),
        CONSTRAINT [FK_historico_inventario_sede_id_sede] FOREIGN KEY ([id_sede]) REFERENCES [sede] ([id_sede]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [ubicacion] (
        [id_ubicacion] int NOT NULL IDENTITY,
        [id_sede] int NOT NULL,
        [nombre] varchar(100) NOT NULL,
        [descripcion] varchar(200) NULL,
        [latitud] decimal(9,6) NOT NULL,
        [longitud] decimal(9,6) NOT NULL,
        [habilitado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_ubicacion] PRIMARY KEY ([id_ubicacion]),
        CONSTRAINT [FK_ubicacion_sede_id_sede] FOREIGN KEY ([id_sede]) REFERENCES [sede] ([id_sede]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [responsable] (
        [id_responsable] int NOT NULL IDENTITY,
        [id_area] int NOT NULL,
        [nombre_completo] varchar(150) NOT NULL,
        [cargo] varchar(100) NULL,
        [correo] varchar(150) NULL,
        [telefono] varchar(30) NULL,
        [habilitado] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_responsable] PRIMARY KEY ([id_responsable]),
        CONSTRAINT [FK_responsable_area_id_area] FOREIGN KEY ([id_area]) REFERENCES [area] ([id_area]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [activo] (
        [id_activo] int NOT NULL IDENTITY,
        [id_categoria] int NOT NULL,
        [id_proveedor] int NOT NULL,
        [id_ubicacion] int NOT NULL,
        [nombre] varchar(150) NOT NULL,
        [descripcion] varchar(300) NULL,
        [marca] varchar(100) NULL,
        [modelo] varchar(100) NULL,
        [numero_serie] varchar(100) NULL,
        [fecha_compra] date NOT NULL,
        [costo_adquisicion] decimal(12,2) NOT NULL,
        [moneda] varchar(10) NULL,
        [numero_factura] varchar(50) NULL,
        [fecha_vencimiento_garantia] date NOT NULL,
        [observaciones] varchar(500) NULL,
        CONSTRAINT [PK_activo] PRIMARY KEY ([id_activo]),
        CONSTRAINT [FK_activo_categoria_activo_id_categoria] FOREIGN KEY ([id_categoria]) REFERENCES [categoria_activo] ([id_categoria]) ON DELETE NO ACTION,
        CONSTRAINT [FK_activo_proveedor_id_proveedor] FOREIGN KEY ([id_proveedor]) REFERENCES [proveedor] ([id_proveedor]) ON DELETE NO ACTION,
        CONSTRAINT [FK_activo_ubicacion_id_ubicacion] FOREIGN KEY ([id_ubicacion]) REFERENCES [ubicacion] ([id_ubicacion]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [asignacion] (
        [id_asignacion] int NOT NULL IDENTITY,
        [id_activo] int NOT NULL,
        [id_usuario] int NOT NULL,
        [id_responsable] int NOT NULL,
        [id_estado] int NOT NULL,
        [id_tipo_asignacion] int NOT NULL,
        [fecha_asignacion] datetime2 NOT NULL,
        [fecha_devolucion] datetime2 NULL,
        [activa] bit NOT NULL DEFAULT CAST(1 AS bit),
        [observaciones] varchar(300) NULL,
        [firma_entrega] varbinary(max) NULL,
        [fecha_firma_entrega] datetime2 NULL,
        [firma_recibe] varbinary(max) NULL,
        [documento_pdf_url] varchar(300) NULL,
        [documento_pdf_generado_en] datetime2 NULL,
        CONSTRAINT [PK_asignacion] PRIMARY KEY ([id_asignacion]),
        CONSTRAINT [FK_asignacion_activo_id_activo] FOREIGN KEY ([id_activo]) REFERENCES [activo] ([id_activo]) ON DELETE NO ACTION,
        CONSTRAINT [FK_asignacion_estado_id_estado] FOREIGN KEY ([id_estado]) REFERENCES [estado] ([id_estado]) ON DELETE NO ACTION,
        CONSTRAINT [FK_asignacion_responsable_id_responsable] FOREIGN KEY ([id_responsable]) REFERENCES [responsable] ([id_responsable]) ON DELETE NO ACTION,
        CONSTRAINT [FK_asignacion_tipo_asignacion_id_tipo_asignacion] FOREIGN KEY ([id_tipo_asignacion]) REFERENCES [tipo_asignacion] ([id_tipo_asignacion]) ON DELETE NO ACTION,
        CONSTRAINT [FK_asignacion_usuario_id_usuario] FOREIGN KEY ([id_usuario]) REFERENCES [usuario] ([id_usuario]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [detalle_activo] (
        [id_detalle] int NOT NULL IDENTITY,
        [id_activo] int NOT NULL,
        [id_inventario] int NOT NULL,
        [encontrado] bit NOT NULL,
        [buen_estado] bit NOT NULL,
        [observaciones] varchar(300) NULL,
        [fecha_verificacion] datetime2 NOT NULL,
        CONSTRAINT [PK_detalle_activo] PRIMARY KEY ([id_detalle]),
        CONSTRAINT [FK_detalle_activo_activo_id_activo] FOREIGN KEY ([id_activo]) REFERENCES [activo] ([id_activo]) ON DELETE NO ACTION,
        CONSTRAINT [FK_detalle_activo_historico_inventario_id_inventario] FOREIGN KEY ([id_inventario]) REFERENCES [historico_inventario] ([id_inventario]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE TABLE [historial_activo] (
        [id_historial_activo] int NOT NULL IDENTITY,
        [id_asignacion] int NULL,
        [id_detalle] int NULL,
        [fecha_hora] datetime2 NOT NULL,
        [tipo_operacion] varchar(30) NULL,
        [descripcion] varchar(300) NULL,
        [informacion_anterior] nvarchar(max) NULL,
        [informacion_nueva] nvarchar(max) NULL,
        CONSTRAINT [PK_historial_activo] PRIMARY KEY ([id_historial_activo]),
        CONSTRAINT [FK_historial_activo_asignacion_id_asignacion] FOREIGN KEY ([id_asignacion]) REFERENCES [asignacion] ([id_asignacion]) ON DELETE NO ACTION,
        CONSTRAINT [FK_historial_activo_detalle_activo_id_detalle] FOREIGN KEY ([id_detalle]) REFERENCES [detalle_activo] ([id_detalle]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_activo_id_categoria] ON [activo] ([id_categoria]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_activo_id_proveedor] ON [activo] ([id_proveedor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_activo_id_ubicacion] ON [activo] ([id_ubicacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_activo_numero_serie] ON [activo] ([numero_serie]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_area_id_sede] ON [area] ([id_sede]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_asignacion_activa] ON [asignacion] ([activa]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_asignacion_id_activo] ON [asignacion] ([id_activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_asignacion_id_estado] ON [asignacion] ([id_estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_asignacion_id_responsable] ON [asignacion] ([id_responsable]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_asignacion_id_tipo_asignacion] ON [asignacion] ([id_tipo_asignacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_asignacion_id_usuario] ON [asignacion] ([id_usuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_bitacora_fecha_hora] ON [bitacora] ([fecha_hora]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_bitacora_id_usuario] ON [bitacora] ([id_usuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_categoria_activo_nombre] ON [categoria_activo] ([nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_detalle_activo_id_activo] ON [detalle_activo] ([id_activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_detalle_activo_id_inventario] ON [detalle_activo] ([id_inventario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_empresa_nit_codigo] ON [empresa] ([nit_codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_estado_nombre] ON [estado] ([nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_historial_activo_fecha_hora] ON [historial_activo] ([fecha_hora]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_historial_activo_id_asignacion] ON [historial_activo] ([id_asignacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_historial_activo_id_detalle] ON [historial_activo] ([id_detalle]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_historico_inventario_fecha_inicio] ON [historico_inventario] ([fecha_inicio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_historico_inventario_id_sede] ON [historico_inventario] ([id_sede]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_pais_codigo_iso2] ON [pais] ([codigo_iso2]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_pais_codigo_iso3] ON [pais] ([codigo_iso3]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_proveedor_id_empresa] ON [proveedor] ([id_empresa]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_proveedor_nit] ON [proveedor] ([nit]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_responsable_id_area] ON [responsable] ([id_area]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_sede_id_empresa] ON [sede] ([id_empresa]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_sede_id_pais] ON [sede] ([id_pais]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_tipo_asignacion_nombre] ON [tipo_asignacion] ([nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_ubicacion_id_sede] ON [ubicacion] ([id_sede]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_ubicacion_nombre] ON [ubicacion] ([nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_usuario_correo] ON [usuario] ([correo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_usuario_id_empresa] ON [usuario] ([id_empresa]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_usuario_username] ON [usuario] ([username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828164949_InitialSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260828164949_InitialSchema', N'8.0.10');
END;
GO

COMMIT;
GO

