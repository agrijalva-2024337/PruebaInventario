using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;

namespace SLCDM.Application.Features.Auth.Commands;

public sealed record LoginCommand(string EmailOrUsername, string Password);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("El campo email o username es obligatorio.")
            .MaximumLength(150).WithMessage("El campo email o username no debe superar los 150 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("El campo password es obligatorio.")
            .MaximumLength(128).WithMessage("El campo password no debe superar los 128 caracteres.");
    }
}

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto>
{
    private static readonly string DummyHash = new PasswordHashService().HashPassword("__timing__");

    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IValidator<LoginCommand> _validator;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHashService passwordHashService,
        IValidator<LoginCommand> validator)
    {
        _db = db;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHashService = passwordHashService;
        _validator = validator;
    }

    public async Task<LoginResponseDto> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var key = command.EmailOrUsername.Trim();
        var usuario = key.Contains('@')
            ? await _db.Usuarios.AsNoTracking().IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == key.ToLowerInvariant(), cancellationToken)
            : await _db.Usuarios.AsNoTracking().IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == key, cancellationToken);

        var hashOk = _passwordHashService.VerifyPassword(command.Password, usuario?.PasswordHash ?? DummyHash);

        if (usuario is null || !usuario.Habilitado || !hashOk)
        {
            throw new UnauthorizedException();
        }

        var jwt = _jwtTokenGenerator.Generate(usuario);
        var rolClaim = usuario.Rol.ToClaimValue();

        return new LoginResponseDto(
            true,
            "Login exitoso",
            jwt.AccessToken,
            "Bearer",
            jwt.ExpiresAtUtc,
            new AuthenticatedUserDto(
                usuario.Id,
                usuario.Username,
                $"{usuario.Nombres} {usuario.Apellidos}".Trim(),
                usuario.Correo,
                usuario.Rol,
                rolClaim,
                usuario.IdEmpresa));
    }
}
