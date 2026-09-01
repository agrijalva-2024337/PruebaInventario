using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.TiposAsignacion.Commands;

public sealed record CreateTipoAsignacionCommand(
    string Nombre,
    string? Descripcion);

public sealed class CreateTipoAsignacionCommandValidator : AbstractValidator<CreateTipoAsignacionCommand>
{
    public CreateTipoAsignacionCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El campo nombre no debe superar los 50 caracteres.")
            .MustAsync(async (nombre, ct) =>
                !await db.TiposAsignacion.AnyAsync(t => t.Nombre == nombre, ct))
            .WithMessage("Ya existe un tipo de asignacion con el mismo nombre.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(150).WithMessage("El campo descripcion no debe superar los 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class CreateTipoAsignacionCommandHandler : ICommandHandler<CreateTipoAsignacionCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateTipoAsignacionCommand> _validator;

    public CreateTipoAsignacionCommandHandler(IApplicationDbContext db, IValidator<CreateTipoAsignacionCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateTipoAsignacionCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<TipoAsignacion>();

        _db.TiposAsignacion.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
