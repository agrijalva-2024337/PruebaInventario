BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828172328_AsignacionUbicacionUsoYFirmaRecibe'
)
BEGIN
    ALTER TABLE [asignacion] ADD [id_ubicacion] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828172328_AsignacionUbicacionUsoYFirmaRecibe'
)
BEGIN
    CREATE INDEX [IX_asignacion_id_ubicacion] ON [asignacion] ([id_ubicacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828172328_AsignacionUbicacionUsoYFirmaRecibe'
)
BEGIN
    ALTER TABLE [asignacion] ADD CONSTRAINT [FK_asignacion_ubicacion_id_ubicacion] FOREIGN KEY ([id_ubicacion]) REFERENCES [ubicacion] ([id_ubicacion]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828172328_AsignacionUbicacionUsoYFirmaRecibe'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260828172328_AsignacionUbicacionUsoYFirmaRecibe', N'8.0.10');
END;
GO

COMMIT;
GO

