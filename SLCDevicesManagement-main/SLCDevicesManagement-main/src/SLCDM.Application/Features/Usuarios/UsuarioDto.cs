using SLCDM.Domain.Enums;

namespace SLCDM.Application.Features.Usuarios;

public sealed record UsuarioDto(
    int Id,
    bool Habilitado,
    int? IdEmpresa,
    string Nombres,
    string Apellidos,
    string Correo,
    string Username,
    RolUsuario Rol,
    DateTime FechaCreacion);

/// <summary>
/// Alta de usuario. <see cref="PasswordGenerada"/> solo viene llena cuando
/// el cliente pidió <c>generarPassword</c>; es la única vez que la API
/// expone la clave en claro.
/// </summary>
public sealed record CreateUsuarioResult(int Id, string? PasswordGenerada);
