using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Responsables.Commands;

public sealed record CreateResponsableCommand(
    int IdArea,
    string NombreCompleto,
    string? Cargo,
    string? Correo,
    string? Telefono);

public sealed class CreateResponsableCommandValidator : AbstractValidator<CreateResponsableCommand>
{
    public CreateResponsableCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdArea)
            .RequiredId("id area")
            .MustAsync(async (id, ct) => await db.Areas.AnyAsync(a => a.Id == id, ct))
            .WithMessage("El campo id area no corresponde a un registro existente.");

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El campo nombre completo es obligatorio.")
            .MaximumLength(150).WithMessage("El campo nombre completo no debe superar los 150 caracteres.");

        RuleFor(x => x.Cargo)
            .MaximumLength(100).WithMessage("El campo cargo no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Cargo));

        RuleFor(x => x.Correo)
            .MaximumLength(150).WithMessage("El campo correo no debe superar los 150 caracteres.")
            .EmailAddress().WithMessage("El formato del correo no es valido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));

        RuleFor(x => x.Telefono)
            .MaximumLength(30).WithMessage("El campo telefono no debe superar los 30 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
    }
}

public sealed class CreateResponsableCommandHandler : ICommandHandler<CreateResponsableCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateResponsableCommand> _validator;

    public CreateResponsableCommandHandler(IApplicationDbContext db, IValidator<CreateResponsableCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateResponsableCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<Responsable>();
        entity.Habilitado = true;

        _db.Responsables.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
