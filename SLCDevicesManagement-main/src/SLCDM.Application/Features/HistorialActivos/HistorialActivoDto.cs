namespace SLCDM.Application.Features.HistorialActivos;

public sealed record HistorialActivoDto(
    int Id,
    int? IdAsignacion,
    int? IdDetalleActivo,
    DateTime FechaHora,
    string? TipoOperacion,
    string? Descripcion,
    string? InformacionAnterior,
    string? InformacionNueva);
