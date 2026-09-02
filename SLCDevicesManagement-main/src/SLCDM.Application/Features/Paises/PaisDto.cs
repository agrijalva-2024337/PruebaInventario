namespace SLCDM.Application.Features.Paises;

public sealed record PaisDto(
    int Id,
    string Nombre,
    string CodigoIso2,
    string CodigoIso3,
    string? CodigoTelefonico);
