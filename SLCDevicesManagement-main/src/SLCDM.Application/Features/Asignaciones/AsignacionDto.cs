namespace SLCDM.Application.Features.Asignaciones;

public sealed record AsignacionDto(
    int Id,
    int IdActivo,
    int IdUsuario,
    int IdResponsable,
    int IdUbicacion,
    int IdEstado,
    int IdTipoAsignacion,
    DateTime FechaAsignacion,
    DateTime? FechaDevolucion,
    bool Activa,
    string? Observaciones,
    bool TieneFirmaEntrega,
    DateTime? FechaFirmaEntrega,
    bool TieneFirmaRecibe,
    string? DocumentoPdfUrl,
    DateTime? DocumentoPdfGenerardoEn);

public sealed record AsignacionHistorialDto(
    int Id,
    int IdActivo,
    int IdUsuario,
    string UsuarioEntrega,
    int IdResponsable,
    string ResponsableRecibe,
    int IdUbicacion,
    string UbicacionUso,
    int IdTipoAsignacion,
    string TipoAsignacion,
    DateTime FechaAsignacion,
    DateTime? FechaDevolucion,
    bool Activa,
    string? Observaciones);

public sealed record AsignacionPdfFileDto(byte[] Content, string FileName);
