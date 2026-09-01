using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;
using SLCDM.Application.Common.Security;
using SLCDM.Domain.Entities;

namespace SLCDM.Api.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public GeneratedJwt Generate(Usuario usuario)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryInMinutes);
        var nombre = $"{usuario.Nombres} {usuario.Apellidos}".Trim();
        var rol = usuario.Rol.ToClaimValue();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(AuthClaimTypes.Name, nombre),
            new(AuthClaimTypes.Email, usuario.Correo),
            new(AuthClaimTypes.Role, rol),
            new(AuthClaimTypes.IdEmpresa, usuario.IdEmpresa?.ToString() ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        return new GeneratedJwt(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
