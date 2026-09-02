using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Estados.Commands;

public sealed record UpdateEstadoCommand(
    int Id,
    string Nombre,
    string? Descripcion);

public sealed class UpdateEstadoCommandValidator : AbstractValidator<UpdateEstadoCommand>
{
    public UpdateEstadoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id estado");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El campo nombre no debe superar los 50 caracteres.")
            .MustAsync(async (cmd, nombre, ct) =>
                !await db.Estados.AnyAsync(e => e.Nombre == nombre && e.Id != cmd.Id, ct))
            .WithMessage("Ya existe un estado con el mismo nombre.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(150).WithMessage("El campo descripcion no debe superar los 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class UpdateEstadoCommandHandler : ICommandHandler<UpdateEstadoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateEstadoCommand> _validator;

    public UpdateEstadoCommandHandler(IApplicationDbContext db, IValidator<UpdateEstadoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateEstadoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Estados.FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Estado", command.Id);

        command.Adapt(entity);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
