namespace SLCDM.Application.Features.Reportes;

public sealed record InventarioEmpresaResumenDto(
    int IdEmpresa,
    string NombreEmpresa,
    int TotalActivos,
    int Disponibles,
    int Asignados,
    int EnMantenimiento,
    int DadosDeBaja,
    decimal CostoAdquisicionTotal);

public sealed record ActivosPorSedeDto(
    int IdSede,
    string NombreSede,
    int IdEmpresa,
    int TotalActivos,
    int Disponibles,
    int Asignados,
    int EnMantenimiento,
    int DadosDeBaja);

public sealed record ActivosPorUbicacionDto(
    int IdUbicacion,
    string NombreUbicacion,
    int IdSede,
    string NombreSede,
    int IdEmpresa,
    int TotalActivos,
    int Disponibles,
    int Asignados,
    int EnMantenimiento,
    int DadosDeBaja);

public sealed record ActivosPorCategoriaDto(
    int IdCategoriaActivo,
    string NombreCategoria,
    int TotalActivos,
    int Disponibles,
    int Asignados,
    int EnMantenimiento,
    int DadosDeBaja);

public sealed record ActivosPorResponsableDto(
    int IdResponsable,
    string NombreResponsable,
    int TotalAsignados);

public sealed record ActivoReporteDto(
    Activos.ActivoDto Activo,
    string EstadoOperativo,
    int IdSede,
    string NombreSede,
    int? IdResponsable);

public sealed record GarantiaPorVencerDto(
    Activos.ActivoDto Activo,
    DateTime FechaVencimientoGarantia,
    int DiasRestantes,
    int IdSede,
    string NombreSede);
