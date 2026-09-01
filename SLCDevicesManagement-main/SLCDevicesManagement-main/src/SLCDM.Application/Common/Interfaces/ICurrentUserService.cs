namespace SLCDM.Application.Common.Interfaces;

/// <summary>
/// Usuario autenticado extraido del JWT. Lo usa el filtro global de
/// empresa en EF Core y cualquier caso de uso que necesite el tenant.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    int? UserId { get; }

    string? Username { get; }

    string? Role { get; }

    int? EmpresaId { get; }

    bool IsAdministradorGeneral { get; }
}
