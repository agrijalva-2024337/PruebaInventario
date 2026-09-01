namespace SLCDM.Application.Features.Estados;

public sealed record EstadoDto(
    int Id,
    string Nombre,
    string? Descripcion);
