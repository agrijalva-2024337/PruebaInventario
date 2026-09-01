using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Application.Features.Asignaciones;
using SLCDM.Application.Features.HistoricosInventario;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.DetallesActivos.Commands;

public sealed record CreateDetalleActivoCommand(
    int IdActivo,
    int IdHistoricoInventario,
    bool Encontrado,
    bool BuenEstado,
    string? Observaciones,
    DateTime FechaVerificacion);

public sealed class CreateDetalleActivoCommandValidator : AbstractValidator<CreateDetalleActivoCommand>
{
    public CreateDetalleActivoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdActivo)
            .RequiredId("id activo")
            .MustAsync(async (id, ct) => await db.Activos.AnyAsync(a => a.Id == id, ct))
            .WithMessage("No se encontro un activo con el id informado.");

        RuleFor(x => x.IdHistoricoInventario)
            .RequiredId("id historico inventario")
            .MustAsync(async (id, ct) => await db.HistoricosInventario.AnyAsync(h => h.Id == id, ct))
            .WithMessage("No se encontro un historico de inventario con el id informado.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                !await db.DetallesActivos.AnyAsync(
                    d => d.IdActivo == cmd.IdActivo && d.IdHistoricoInventario == cmd.IdHistoricoInventario,
                    ct))
            .WithMessage("Ya existe un detalle de activo para este activo e inventario.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                await InventarioJornadaRules.ActivoEnSedeDeJornadaAsync(
                    db, cmd.IdActivo, cmd.IdHistoricoInventario, ct))
            .WithMessage("El activo no pertenece a la sede de la jornada. La ubicacion teorica debe ser de la misma sede.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await ActivoBajaRules.EstaDadoDeBajaAsync(db, cmd.IdActivo, ct))
            .WithMessage("El activo esta dado de baja y no puede incluirse en la jornada.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}

public sealed class CreateDetalleActivoCommandHandler : ICommandHandler<CreateDetalleActivoCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateDetalleActivoCommand> _validator;

    public CreateDetalleActivoCommandHandler(
        IApplicationDbContext db,
        IValidator<CreateDetalleActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateDetalleActivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var jornada = await _db.HistoricosInventario
            .AsNoTracking()
            .FirstAsync(h => h.Id == command.IdHistoricoInventario, cancellationToken);
        if (jornada.Cerrado)
        {
            throw new ConflictException("No se puede registrar el detalle porque el historico de inventario esta cerrado.");
        }

        if (!await InventarioJornadaRules.ActivoEnSedeDeJornadaAsync(
                _db, command.IdActivo, command.IdHistoricoInventario, cancellationToken))
        {
            throw new ConflictException(
                "El activo no pertenece a la sede de la jornada. La ubicacion teorica debe ser de la misma sede.");
        }

        if (await ActivoBajaRules.EstaDadoDeBajaAsync(_db, command.IdActivo, cancellationToken))
        {
            throw new ConflictException("El activo esta dado de baja y no puede incluirse en la jornada.");
        }

        var entity = command.Adapt<DetalleActivo>();
        if (entity.FechaVerificacion == default)
        {
            entity.FechaVerificacion = DateTime.UtcNow;
        }

        _db.DetallesActivos.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
