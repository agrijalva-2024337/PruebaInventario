namespace SLCDM.Application.Features.Sedes;

public sealed record SedeDto(
    int Id,
    bool Habilitado,
    int IdEmpresa,
    int IdPais,
    string Nombre,
    string? Direccion,
    string? Ciudad);
