namespace SLCDM.Application.Features.HistoricosInventario;

public sealed record HistoricoInventarioDto(
    int Id,
    int IdSede,
    bool Cerrado,
    string? Responsable,
    DateTime FechaInicio,
    DateTime? FechaCierre,
    string? Observaciones);
