using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;

namespace SLCDM.Api.Authentication;

public sealed class DeviceTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApplicationDbContext _db;
    private readonly IDeviceTokenHashService _tokenHash;

    public DeviceTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApplicationDbContext db,
        IDeviceTokenHashService tokenHash)
        : base(options, logger, encoder)
    {
        _db = db;
        _tokenHash = tokenHash;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(DeviceTokenDefaults.HeaderName, out var headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        var rawToken = headerValue.ToString();
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return AuthenticateResult.Fail("Token de dispositivo vacio.");
        }

        var hash = _tokenHash.Hash(rawToken);
        var ahora = DateTime.UtcNow;

        var token = await _db.DispositivosToken.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.TokenHash == hash && !d.Revocado
                && (d.ExpiraEn == null || d.ExpiraEn > ahora));

        if (token is null)
        {
            return AuthenticateResult.Fail("Token de dispositivo invalido, revocado o expirado.");
        }

        token.UltimoUsoEn = ahora;
        await _db.SaveChangesAsync(CancellationToken.None);

        var claims = new[] { new Claim(DeviceClaimTypes.IdActivo, token.IdActivo.ToString()) };
        var identity = new ClaimsIdentity(claims, DeviceTokenDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, DeviceTokenDefaults.AuthenticationScheme);

        return AuthenticateResult.Success(ticket);
    }
}
