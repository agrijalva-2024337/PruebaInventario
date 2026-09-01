namespace SLCDM.Application.Features.DetallesActivos;

public sealed record DetalleActivoDto(
    int Id,
    int IdActivo,
    int IdHistoricoInventario,
    bool Encontrado,
    bool BuenEstado,
    string? Observaciones,
    DateTime FechaVerificacion);
