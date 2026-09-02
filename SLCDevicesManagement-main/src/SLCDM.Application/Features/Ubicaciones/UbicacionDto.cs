namespace SLCDM.Application.Features.Ubicaciones;

public sealed record UbicacionDto(
    int Id,
    bool Habilitado,
    int IdSede,
    string Nombre,
    string? Descripcion,
    decimal Latitud,
    decimal Longitud);
