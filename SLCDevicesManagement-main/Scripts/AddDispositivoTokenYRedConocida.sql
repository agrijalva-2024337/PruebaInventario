-- Tablas de rastreo de ubicacion (idempotente). Preferir:
-- dotnet ef database update --project src/SLCDM.Persistence --startup-project src/SLCDM.Api

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.dispositivo_token', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[dispositivo_token] (
        [id_dispositivo_token] INT IDENTITY(1,1) NOT NULL,
        [id_activo] INT NOT NULL,
        [token_hash] VARCHAR(200) NOT NULL,
        [creado_en] DATETIME2 NOT NULL,
        [expira_en] DATETIME2 NULL,
        [ultimo_uso_en] DATETIME2 NULL,
        [revocado] BIT NOT NULL CONSTRAINT [DF_dispositivo_token_revocado] DEFAULT (0),
        [ultima_ubicacion_detectada_id] INT NULL,
        [fuera_de_rango] BIT NOT NULL CONSTRAINT [DF_dispositivo_token_fuera_de_rango] DEFAULT (0),
        CONSTRAINT [PK_dispositivo_token] PRIMARY KEY ([id_dispositivo_token]),
        CONSTRAINT [FK_dispositivo_token_activo_id_activo]
            FOREIGN KEY ([id_activo]) REFERENCES [dbo].[activo] ([id_activo]) ON DELETE CASCADE,
        CONSTRAINT [FK_dispositivo_token_ubicacion_ultima_ubicacion_detectada_id]
            FOREIGN KEY ([ultima_ubicacion_detectada_id]) REFERENCES [dbo].[ubicacion] ([id_ubicacion])
    );

    CREATE UNIQUE INDEX [IX_dispositivo_token_token_hash]
        ON [dbo].[dispositivo_token] ([token_hash]);

    CREATE UNIQUE INDEX [IX_dispositivo_token_id_activo]
        ON [dbo].[dispositivo_token] ([id_activo])
        WHERE [revocado] = 0;

    CREATE INDEX [IX_dispositivo_token_ultima_ubicacion_detectada_id]
        ON [dbo].[dispositivo_token] ([ultima_ubicacion_detectada_id]);
END;

IF OBJECT_ID(N'dbo.red_conocida', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[red_conocida] (
        [id_red_conocida] INT IDENTITY(1,1) NOT NULL,
        [bssid] VARCHAR(17) NOT NULL,
        [id_ubicacion] INT NOT NULL,
        CONSTRAINT [PK_red_conocida] PRIMARY KEY ([id_red_conocida]),
        CONSTRAINT [FK_red_conocida_ubicacion_id_ubicacion]
            FOREIGN KEY ([id_ubicacion]) REFERENCES [dbo].[ubicacion] ([id_ubicacion]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_red_conocida_bssid]
        ON [dbo].[red_conocida] ([bssid]);

    CREATE INDEX [IX_red_conocida_id_ubicacion]
        ON [dbo].[red_conocida] ([id_ubicacion]);
END;
GO
