namespace SLCDM.Application.Features.TiposAsignacion;

public sealed record TipoAsignacionDto(
    int Id,
    string Nombre,
    string? Descripcion);
