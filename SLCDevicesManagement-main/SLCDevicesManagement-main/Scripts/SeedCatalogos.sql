-- Catalogos minimos DERCAS. Idempotente (IF NOT EXISTS por nombre).
-- Requerido para BE-16/17/18: sin estos tipos, traslado/mantenimiento/baja responden 409.
-- Los nombres deben coincidir con SLCDM.Application.Features.Asignaciones.TipoAsignacionNombres.

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM [tipo_asignacion] WHERE [nombre] = N'Asignacion')
BEGIN
    INSERT INTO [tipo_asignacion] ([nombre], [descripcion])
    VALUES (N'Asignacion', N'Entrega a responsable. Ocupa el activo.');
END;

IF NOT EXISTS (SELECT 1 FROM [tipo_asignacion] WHERE [nombre] = N'Traslado')
BEGIN
    INSERT INTO [tipo_asignacion] ([nombre], [descripcion])
    VALUES (N'Traslado', N'Cambio de ubicacion. No ocupa el activo.');
END;

IF NOT EXISTS (SELECT 1 FROM [tipo_asignacion] WHERE [nombre] = N'Mantenimiento')
BEGIN
    INSERT INTO [tipo_asignacion] ([nombre], [descripcion])
    VALUES (N'Mantenimiento', N'Envio a mantenimiento. Ocupa el activo.');
END;

IF NOT EXISTS (SELECT 1 FROM [tipo_asignacion] WHERE [nombre] = N'Baja')
BEGIN
    INSERT INTO [tipo_asignacion] ([nombre], [descripcion])
    VALUES (N'Baja', N'Baja operativa. El activo no se elimina.');
END;

-- Al menos un Estado: Asignacion.IdEstado es FK obligatorio en entrega/traslado/mantenimiento/baja.
IF NOT EXISTS (SELECT 1 FROM [estado] WHERE [nombre] = N'Activo')
BEGIN
    INSERT INTO [estado] ([nombre], [descripcion])
    VALUES (N'Activo', N'Estado operativo por defecto para movimientos.');
END;
GO
