using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Auth;

namespace SLCDM.Application.Features.Auth.Queries;

public sealed record GetMeQuery;

public sealed class GetMeQueryHandler : IQueryHandler<GetMeQuery, AuthenticatedUserDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMeQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AuthenticatedUserDto> HandleAsync(GetMeQuery query, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken)
            ?? throw new NotFoundException("Usuario", _currentUser.UserId.Value);

        if (!usuario.Habilitado)
        {
            throw new UnauthorizedException();
        }

        return new AuthenticatedUserDto(
            usuario.Id,
            usuario.Username,
            $"{usuario.Nombres} {usuario.Apellidos}".Trim(),
            usuario.Correo,
            usuario.Rol,
            usuario.Rol.ToClaimValue(),
            usuario.IdEmpresa);
    }
}
