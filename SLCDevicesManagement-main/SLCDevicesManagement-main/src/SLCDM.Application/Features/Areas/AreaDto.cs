namespace SLCDM.Application.Features.Areas;

public sealed record AreaDto(
    int Id,
    bool Habilitado,
    int IdSede,
    string Nombre,
    string? Descripcion);
