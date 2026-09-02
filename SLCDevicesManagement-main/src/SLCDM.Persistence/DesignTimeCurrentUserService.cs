using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Persistence;

/// <summary>
/// Usado por el constructor sin HTTP (migraciones / design-time).
/// Se comporta como Administrador general para no ocultar tablas al generar el modelo.
/// </summary>
internal sealed class DesignTimeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated => false;

    public int? UserId => null;

    public string? Username => null;

    public string? Role => null;

    public int? EmpresaId => null;

    public bool IsAdministradorGeneral => true;
}
