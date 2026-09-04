namespace SLCDM.Application.Common.Security;

/// <summary>
/// Nombre de la claim que porta el principal autenticado por DeviceToken
/// (esquema aparte del JWT de usuario: aqui no hay usuario logueado).
/// </summary>
public static class DeviceClaimTypes
{
    public const string IdActivo = "id_activo_dispositivo";
}
