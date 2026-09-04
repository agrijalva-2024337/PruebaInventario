using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;

namespace SLCDM.Application.Common.Security;

/// <summary>
/// Hash de tokens de dispositivo. A diferencia de PasswordHashService
/// (Argon2, pensado para contrasenas de baja entropia elegidas por
/// humanos), un token de dispositivo es un secreto aleatorio de alta
/// entropia: aqui se necesita un hash deterministico (HMAC-SHA256 con
/// pepper del servidor) para poder buscarlo por igualdad en la base de
/// datos. Un token de 32 bytes aleatorios no es fuerza-bruteable aunque el
/// hash sea rapido de calcular.
/// </summary>
public sealed class DeviceTokenHashService : IDeviceTokenHashService
{
    private readonly byte[] _pepper;

    public DeviceTokenHashService(IOptions<DeviceTrackingOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Pepper) || options.Value.Pepper.Length < 32)
        {
            throw new InvalidOperationException(
                "DeviceTracking:Pepper debe tener al menos 32 caracteres. Use User Secrets o variables de entorno en produccion.");
        }

        _pepper = Encoding.UTF8.GetBytes(options.Value.Pepper);
    }

    public string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string Hash(string rawToken)
    {
        using var hmac = new HMACSHA256(_pepper);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash);
    }
}
