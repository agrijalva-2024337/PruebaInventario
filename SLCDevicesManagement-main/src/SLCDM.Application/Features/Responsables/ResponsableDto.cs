namespace SLCDM.Application.Features.Responsables;

public sealed record ResponsableDto(
    int Id,
    bool Habilitado,
    int IdArea,
    string NombreCompleto,
    string? Cargo,
    string? Correo,
    string? Telefono);
