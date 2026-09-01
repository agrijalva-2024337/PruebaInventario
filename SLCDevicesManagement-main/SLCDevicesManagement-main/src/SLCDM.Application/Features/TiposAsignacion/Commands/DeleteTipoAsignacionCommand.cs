using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.TiposAsignacion.Commands;

public sealed record DeleteTipoAsignacionCommand(int Id);

public sealed class DeleteTipoAsignacionCommandValidator : AbstractValidator<DeleteTipoAsignacionCommand>
{
    public DeleteTipoAsignacionCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id tipo asignacion");
    }
}

public sealed class DeleteTipoAsignacionCommandHandler : ICommandHandler<DeleteTipoAsignacionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DeleteTipoAsignacionCommand> _validator;

    public DeleteTipoAsignacionCommandHandler(IApplicationDbContext db, IValidator<DeleteTipoAsignacionCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DeleteTipoAsignacionCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.TiposAsignacion.FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("TipoAsignacion", command.Id);

        var enUso = await _db.Asignaciones.AnyAsync(a => a.IdTipoAsignacion == command.Id, cancellationToken);
        if (enUso)
        {
            throw new ConflictException("No se puede eliminar el tipo de asignacion porque tiene asignaciones asociadas.");
        }

        _db.TiposAsignacion.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
