using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.DetallesActivos.Commands;

public sealed record UpdateDetalleActivoCommand(
    int Id,
    bool Encontrado,
    bool BuenEstado,
    string? Observaciones,
    DateTime FechaVerificacion);

public sealed class UpdateDetalleActivoCommandValidator : AbstractValidator<UpdateDetalleActivoCommand>
{
    public UpdateDetalleActivoCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id detalle activo");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}

public sealed class UpdateDetalleActivoCommandHandler : ICommandHandler<UpdateDetalleActivoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateDetalleActivoCommand> _validator;

    public UpdateDetalleActivoCommandHandler(
        IApplicationDbContext db,
        IValidator<UpdateDetalleActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateDetalleActivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.DetallesActivos
            .Include(d => d.HistoricoInventario)
            .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("DetalleActivo", command.Id);

        if (entity.HistoricoInventario is { Cerrado: true })
        {
            throw new ConflictException("No se puede actualizar el detalle porque el historico de inventario esta cerrado.");
        }

        command.Adapt(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
