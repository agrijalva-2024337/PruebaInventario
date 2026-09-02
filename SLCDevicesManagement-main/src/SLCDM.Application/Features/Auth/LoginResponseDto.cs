using SLCDM.Domain.Enums;

namespace SLCDM.Application.Features.Auth;

public sealed record LoginResponseDto(
    bool Success,
    string Message,
    string Token,
    string TokenType,
    DateTime ExpiresAt,
    AuthenticatedUserDto UserDetails);

public sealed record AuthenticatedUserDto(
    int Id,
    string Username,
    string Nombre,
    string Email,
    RolUsuario Rol,
    string Role,
    int? IdEmpresa);
