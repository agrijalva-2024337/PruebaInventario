using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.DetallesActivos.Commands;

public sealed record DeleteDetalleActivoCommand(int Id);

public sealed class DeleteDetalleActivoCommandValidator : AbstractValidator<DeleteDetalleActivoCommand>
{
    public DeleteDetalleActivoCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id detalle activo");
    }
}

public sealed class DeleteDetalleActivoCommandHandler : ICommandHandler<DeleteDetalleActivoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DeleteDetalleActivoCommand> _validator;

    public DeleteDetalleActivoCommandHandler(
        IApplicationDbContext db,
        IValidator<DeleteDetalleActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DeleteDetalleActivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.DetallesActivos
            .Include(d => d.HistoricoInventario)
            .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("DetalleActivo", command.Id);

        if (entity.HistoricoInventario is { Cerrado: true })
        {
            throw new ConflictException("No se puede eliminar el detalle porque el historico de inventario esta cerrado.");
        }

        var enUso = await _db.HistorialActivos.AnyAsync(h => h.IdDetalleActivo == command.Id, cancellationToken);
        if (enUso)
        {
            throw new ConflictException("No se puede eliminar el detalle de activo porque tiene historial asociado.");
        }

        _db.DetallesActivos.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
