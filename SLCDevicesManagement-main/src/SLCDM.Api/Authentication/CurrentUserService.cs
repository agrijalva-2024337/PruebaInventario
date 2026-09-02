using System.Security.Claims;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;

namespace SLCDM.Api.Authentication;

public sealed class CurrentUserService : ICurrentUserService
{
    public const string EmpresaActivaHeader = "X-Empresa-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public int? UserId => ParseInt(Find(AuthClaimTypes.Sub) ?? Find(ClaimTypes.NameIdentifier));

    public string? Username => Find(AuthClaimTypes.Name) ?? User?.Identity?.Name;

    public string? Role => Find(AuthClaimTypes.Role) ?? Find(ClaimTypes.Role);

    public int? EmpresaId
    {
        get
        {
            var jwtEmpresa = ParseInt(Find(AuthClaimTypes.IdEmpresa));
            if (!IsAdministradorGeneral)
            {
                return jwtEmpresa;
            }

            var header = _httpContextAccessor.HttpContext?.Request.Headers[EmpresaActivaHeader].ToString();
            return ParseInt(header) ?? jwtEmpresa;
        }
    }

    public bool IsAdministradorGeneral =>
        string.Equals(Role, Roles.AdministradorGeneral, StringComparison.Ordinal);

    private string? Find(string claimType) => User?.FindFirstValue(claimType);

    private static int? ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value, out var parsed) ? parsed : null;
    }
}
