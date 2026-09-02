using SLCDM.Domain.Entities;

namespace SLCDM.Application.Common.Interfaces;

public sealed record GeneratedJwt(string AccessToken, DateTime ExpiresAtUtc);

/// <summary>
/// Genera el JWT de acceso. La implementacion (firma HMAC, issuer, expiracion)
/// vive fuera de Application para no acoplar el caso de uso a
/// Microsoft.IdentityModel.
/// </summary>
public interface IJwtTokenGenerator
{
    GeneratedJwt Generate(Usuario usuario);
}
