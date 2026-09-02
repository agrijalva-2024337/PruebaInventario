using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Proveedores.Commands;

public sealed record CreateProveedorCommand(
    int IdEmpresa,
    string Nombre,
    string Nit,
    string? NombreContacto,
    string? Telefono,
    string? Correo);

public sealed class CreateProveedorCommandValidator : AbstractValidator<CreateProveedorCommand>
{
    public CreateProveedorCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdEmpresa)
            .RequiredId("id empresa")
            .MustAsync(async (id, ct) => await db.Empresas.AnyAsync(e => e.Id == id, ct))
            .WithMessage("No se encontro una empresa con el id informado.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El campo nombre no debe superar los 150 caracteres.");

        RuleFor(x => x.Nit)
            .NotEmpty().WithMessage("El campo nit es obligatorio.")
            .MaximumLength(50).WithMessage("El campo nit no debe superar los 50 caracteres.");

        RuleFor(x => x.NombreContacto)
            .MaximumLength(100).WithMessage("El campo nombre contacto no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.NombreContacto));

        RuleFor(x => x.Telefono)
            .MaximumLength(30).WithMessage("El campo telefono no debe superar los 30 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));

        RuleFor(x => x.Correo)
            .MaximumLength(150).WithMessage("El campo correo no debe superar los 150 caracteres.")
            .EmailAddress().WithMessage("El campo correo no es un correo valido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));
    }
}

public sealed class CreateProveedorCommandHandler : ICommandHandler<CreateProveedorCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateProveedorCommand> _validator;

    public CreateProveedorCommandHandler(IApplicationDbContext db, IValidator<CreateProveedorCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateProveedorCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<Proveedor>();
        entity.Habilitado = true;

        _db.Proveedores.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
