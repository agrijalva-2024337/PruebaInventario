using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Activos.Commands;

public sealed record DeleteActivoCommand(int Id);

public sealed class DeleteActivoCommandValidator : AbstractValidator<DeleteActivoCommand>
{
    public DeleteActivoCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id activo");
    }
}

public sealed class DeleteActivoCommandHandler : ICommandHandler<DeleteActivoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DeleteActivoCommand> _validator;

    public DeleteActivoCommandHandler(IApplicationDbContext db, IValidator<DeleteActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DeleteActivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Activos.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Activo", command.Id);

        var enUso = await _db.Asignaciones.AnyAsync(a => a.IdActivo == command.Id, cancellationToken)
            || await _db.DetallesActivos.AnyAsync(d => d.IdActivo == command.Id, cancellationToken);

        if (enUso)
        {
            throw new ConflictException("No se puede eliminar el activo porque tiene asignaciones o detalles asociados.");
        }

        _db.Activos.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
