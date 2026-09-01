using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Estados.Commands;

public sealed record DeleteEstadoCommand(int Id);

public sealed class DeleteEstadoCommandValidator : AbstractValidator<DeleteEstadoCommand>
{
    public DeleteEstadoCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id estado");
    }
}

public sealed class DeleteEstadoCommandHandler : ICommandHandler<DeleteEstadoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DeleteEstadoCommand> _validator;

    public DeleteEstadoCommandHandler(IApplicationDbContext db, IValidator<DeleteEstadoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DeleteEstadoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Estados.FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Estado", command.Id);

        var enUso = await _db.Asignaciones.AnyAsync(a => a.IdEstado == command.Id, cancellationToken);
        if (enUso)
        {
            throw new ConflictException("No se puede eliminar el estado porque tiene asignaciones asociadas.");
        }

        _db.Estados.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
