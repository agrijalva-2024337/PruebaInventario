using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;
using SLCDM.Domain.Enums;

namespace SLCDM.Application.Features.Usuarios.Commands;

public sealed record CreateUsuarioCommand(
    int? IdEmpresa,
    string Nombres,
    string Apellidos,
    string Correo,
    string Username,
    string? Password,
    RolUsuario Rol,
    bool GenerarPassword = false);

public sealed class CreateUsuarioCommandValidator : AbstractValidator<CreateUsuarioCommand>
{
    public CreateUsuarioCommandValidator(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        RuleFor(x => x.IdEmpresa)
            .OptionalId("id empresa")
            .MustAsync(async (id, ct) =>
                !id.HasValue || await db.Empresas.AnyAsync(e => e.Id == id.Value, ct))
            .WithMessage("El campo id empresa no corresponde a un registro existente.");

        RuleFor(x => x.IdEmpresa)
            .NotNull()
            .WithMessage("El campo id empresa es obligatorio para este rol.")
            .When(x => x.Rol != RolUsuario.AdministradorGeneral);

        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("El campo nombres es obligatorio.")
            .MaximumLength(100).WithMessage("El campo nombres no debe superar los 100 caracteres.");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("El campo apellidos es obligatorio.")
            .MaximumLength(100).WithMessage("El campo apellidos no debe superar los 100 caracteres.");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El campo correo es obligatorio.")
            .MaximumLength(150).WithMessage("El campo correo no debe superar los 150 caracteres.")
            .EmailAddress().WithMessage("El formato del correo no es valido.")
            .MustAsync(async (correo, ct) =>
            {
                var normalized = correo.Trim().ToLowerInvariant();
                return !await db.Usuarios.IgnoreQueryFilters()
                    .AnyAsync(u => u.Correo.ToLower() == normalized, ct);
            })
            .WithMessage("Ya existe un usuario con el mismo correo.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El campo username es obligatorio.")
            .MaximumLength(50).WithMessage("El campo username no debe superar los 50 caracteres.")
            .MustAsync(async (username, ct) =>
                !await db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Username == username, ct))
            .WithMessage("Ya existe un usuario con el mismo username.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("El campo password es obligatorio.")
            .MinimumLength(8).WithMessage("El campo password debe tener al menos 8 caracteres.")
            .MaximumLength(128).WithMessage("El campo password no debe superar los 128 caracteres.")
            .When(x => !x.GenerarPassword);

        RuleFor(x => x.Rol)
            .IsInEnum().WithMessage("El campo rol no es un valor valido.")
            .Must(rol => currentUser.IsAdministradorGeneral || rol != RolUsuario.AdministradorGeneral)
            .WithMessage("Solo el administrador general puede asignar ese rol.");
    }
}

public sealed class CreateUsuarioCommandHandler : ICommandHandler<CreateUsuarioCommand, CreateUsuarioResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IValidator<CreateUsuarioCommand> _validator;

    public CreateUsuarioCommandHandler(
        IApplicationDbContext db,
        IPasswordHashService passwordHashService,
        IPasswordGenerator passwordGenerator,
        IValidator<CreateUsuarioCommand> validator)
    {
        _db = db;
        _passwordHashService = passwordHashService;
        _passwordGenerator = passwordGenerator;
        _validator = validator;
    }

    public async Task<CreateUsuarioResult> HandleAsync(
        CreateUsuarioCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        string? passwordGenerada = null;
        var plain = command.Password;
        if (command.GenerarPassword)
        {
            passwordGenerada = _passwordGenerator.Generate();
            plain = passwordGenerada;
        }

        var entity = command.Adapt<Usuario>();
        entity.Correo = command.Correo.Trim().ToLowerInvariant();
        entity.PasswordHash = _passwordHashService.HashPassword(plain!);
        entity.Habilitado = true;
        entity.FechaCreacion = DateTime.UtcNow;

        _db.Usuarios.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return new CreateUsuarioResult(entity.Id, passwordGenerada);
    }
}
