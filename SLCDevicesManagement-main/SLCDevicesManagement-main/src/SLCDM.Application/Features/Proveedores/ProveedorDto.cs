namespace SLCDM.Application.Features.Proveedores;

public sealed record ProveedorDto(
    int Id,
    bool Habilitado,
    int IdEmpresa,
    string Nombre,
    string Nit,
    string? NombreContacto,
    string? Telefono,
    string? Correo);
