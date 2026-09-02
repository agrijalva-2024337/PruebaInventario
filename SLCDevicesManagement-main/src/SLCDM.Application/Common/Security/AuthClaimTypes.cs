namespace SLCDM.Application.Common.Security;

/// <summary>
/// Nombres de claim del JWT de acceso (contrato de autenticacion DERCAS).
/// </summary>
public static class AuthClaimTypes
{
    public const string Sub = "sub";
    public const string Name = "name";
    public const string Email = "email";
    public const string Role = "role";
    public const string IdEmpresa = "id_empresa";
}
