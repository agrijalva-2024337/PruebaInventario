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
    byte[]? FirmaEntrega,
    DateTime? FechaFirmaEntrega,
    byte[]? FirmaRecibe,
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
