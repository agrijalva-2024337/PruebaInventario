namespace SLCDM.Application.Common.Exceptions;

/// <summary>
/// Credenciales invalidas o usuario inhabilitado. El mensaje es generico a
/// proposito para no revelar si el username existe.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Credenciales invalidas.")
    {
    }
}
