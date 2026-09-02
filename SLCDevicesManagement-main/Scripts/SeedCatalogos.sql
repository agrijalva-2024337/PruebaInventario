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

-- Estados automaticos por tipo de movimiento (SLCDM.Application.Features.Asignaciones.EstadoNombres).
IF NOT EXISTS (SELECT 1 FROM [estado] WHERE [nombre] = N'Disponible')
BEGIN
    INSERT INTO [estado] ([nombre], [descripcion])
    VALUES (N'Disponible', N'Activo libre para asignar. Estado inicial al dar de alta.');
END;

IF NOT EXISTS (SELECT 1 FROM [estado] WHERE [nombre] = N'Asignado')
BEGIN
    INSERT INTO [estado] ([nombre], [descripcion])
    VALUES (N'Asignado', N'Entregado a un responsable. Se aplica al registrar una asignacion.');
END;

IF NOT EXISTS (SELECT 1 FROM [estado] WHERE [nombre] = N'Traslado')
BEGIN
    INSERT INTO [estado] ([nombre], [descripcion])
    VALUES (N'Traslado', N'Estado del movimiento de cambio de ubicacion.');
END;

IF NOT EXISTS (SELECT 1 FROM [estado] WHERE [nombre] = N'Mantenimiento')
BEGIN
    INSERT INTO [estado] ([nombre], [descripcion])
    VALUES (N'Mantenimiento', N'Activo en mantenimiento. Se aplica al iniciar el proceso.');
END;

IF NOT EXISTS (SELECT 1 FROM [estado] WHERE [nombre] = N'Baja')
BEGIN
    INSERT INTO [estado] ([nombre], [descripcion])
    VALUES (N'Baja', N'Dado de baja operativa. El registro no se elimina.');
END;
GO
