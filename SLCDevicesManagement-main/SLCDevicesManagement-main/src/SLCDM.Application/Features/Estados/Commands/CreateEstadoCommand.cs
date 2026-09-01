using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Estados.Commands;

public sealed record CreateEstadoCommand(
    string Nombre,
    string? Descripcion);

public sealed class CreateEstadoCommandValidator : AbstractValidator<CreateEstadoCommand>
{
    public CreateEstadoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El campo nombre no debe superar los 50 caracteres.")
            .MustAsync(async (nombre, ct) =>
                !await db.Estados.AnyAsync(e => e.Nombre == nombre, ct))
            .WithMessage("Ya existe un estado con el mismo nombre.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(150).WithMessage("El campo descripcion no debe superar los 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class CreateEstadoCommandHandler : ICommandHandler<CreateEstadoCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateEstadoCommand> _validator;

    public CreateEstadoCommandHandler(IApplicationDbContext db, IValidator<CreateEstadoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateEstadoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<Estado>();

        _db.Estados.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
