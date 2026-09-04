-- Ultima posicion reportada por cada activo (idempotente). Preferir:
-- dotnet ef database update --project src/SLCDM.Persistence --startup-project src/SLCDM.Api

SET NOCOUNT ON;

IF COL_LENGTH('dbo.dispositivo_token', 'ultimo_bssid') IS NULL
    ALTER TABLE [dbo].[dispositivo_token] ADD [ultimo_bssid] VARCHAR(17) NULL;

IF COL_LENGTH('dbo.dispositivo_token', 'ultima_latitud') IS NULL
    ALTER TABLE [dbo].[dispositivo_token] ADD [ultima_latitud] DECIMAL(9,6) NULL;

IF COL_LENGTH('dbo.dispositivo_token', 'ultima_longitud') IS NULL
    ALTER TABLE [dbo].[dispositivo_token] ADD [ultima_longitud] DECIMAL(9,6) NULL;

IF COL_LENGTH('dbo.dispositivo_token', 'origen_coordenada') IS NULL
    ALTER TABLE [dbo].[dispositivo_token] ADD [origen_coordenada] VARCHAR(10) NULL;
GO
