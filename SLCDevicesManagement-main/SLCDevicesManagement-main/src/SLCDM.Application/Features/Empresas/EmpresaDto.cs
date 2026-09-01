namespace SLCDM.Application.Features.Empresas;

public sealed record EmpresaDto(
    int Id,
    bool Habilitado,
    string Nombre,
    string NitCodigo,
    string? Direccion,
    string? Telefono);
