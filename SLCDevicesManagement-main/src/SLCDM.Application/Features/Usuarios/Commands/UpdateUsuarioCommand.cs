using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Enums;

namespace SLCDM.Application.Features.Usuarios.Commands;

public sealed record UpdateUsuarioCommand(
    int Id,
    int? IdEmpresa,
    string Nombres,
    string Apellidos,
    string Correo,
    string Username,
    string? Password,
    RolUsuario Rol,
    bool Habilitado);

public sealed class UpdateUsuarioCommandValidator : AbstractValidator<UpdateUsuarioCommand>
{
    public UpdateUsuarioCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id usuario");

        RuleFor(x => x.IdEmpresa)
            .OptionalId("id empresa")
            .MustAsync(async (id, ct) =>
                !id.HasValue || await db.Empresas.AnyAsync(e => e.Id == id.Value, ct))
            .WithMessage("El campo id empresa no corresponde a un registro existente.");

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
            .MustAsync(async (cmd, correo, ct) =>
            {
                var normalized = correo.Trim().ToLowerInvariant();
                return !await db.Usuarios.IgnoreQueryFilters()
                    .AnyAsync(u => u.Correo.ToLower() == normalized && u.Id != cmd.Id, ct);
            })
            .WithMessage("Ya existe un usuario con el mismo correo.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El campo username es obligatorio.")
            .MaximumLength(50).WithMessage("El campo username no debe superar los 50 caracteres.")
            .MustAsync(async (cmd, username, ct) =>
                !await db.Usuarios.IgnoreQueryFilters()
                    .AnyAsync(u => u.Username == username && u.Id != cmd.Id, ct))
            .WithMessage("Ya existe un usuario con el mismo username.");

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("El campo password debe tener al menos 8 caracteres.")
            .MaximumLength(128).WithMessage("El campo password no debe superar los 128 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x.Rol)
            .IsInEnum().WithMessage("El campo rol no es un valor valido.");
    }
}

public sealed class UpdateUsuarioCommandHandler : ICommandHandler<UpdateUsuarioCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IValidator<UpdateUsuarioCommand> _validator;

    public UpdateUsuarioCommandHandler(
        IApplicationDbContext db,
        IPasswordHashService passwordHashService,
        IValidator<UpdateUsuarioCommand> validator)
    {
        _db = db;
        _passwordHashService = passwordHashService;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Usuario", command.Id);

        command.Adapt(entity);
        entity.Correo = command.Correo.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(command.Password))
        {
            entity.PasswordHash = _passwordHashService.HashPassword(command.Password);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
