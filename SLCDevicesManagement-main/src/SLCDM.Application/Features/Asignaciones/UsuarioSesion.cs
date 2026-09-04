using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Asignaciones.Commands;

internal static class UsuarioSesion
{
    public static int IdQuienEntrega(ICurrentUserService currentUser)
    {
        if (currentUser.UserId is not int id || id <= 0)
        {
            throw new UnauthorizedException();
        }

        return id;
    }
}
