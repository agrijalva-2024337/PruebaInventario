using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones.Commands;

/// <summary>
/// Cierre de mantenimiento (BE-17): desactiva la asignacion y revierte
/// <c>id_estado</c> al valor previo capturado en <c>historial_activo</c>.
/// </summary>
public sealed record FinalizarMantenimientoCommand(
    int Id,
    DateTime? FechaDevolucion,
    string? Observaciones,
    int? IdEstado);

public sealed class FinalizarMantenimientoCommandValidator : AbstractValidator<FinalizarMantenimientoCommand>
{
    public FinalizarMantenimientoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id asignacion");

        RuleFor(x => x.IdEstado)
            .OptionalId("id estado")
            .MustAsync(async (id, ct) =>
                !id.HasValue || await db.Estados.AnyAsync(e => e.Id == id.Value, ct))
            .WithMessage("No se encontro un estado con el id informado.");

        RuleFor(x => x.Observaciones)
            .NotEmpty().WithMessage("El trabajo realizado es obligatorio.")
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.");
    }
}

public sealed class FinalizarMantenimientoCommandHandler : ICommandHandler<FinalizarMantenimientoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<FinalizarMantenimientoCommand> _validator;

    public FinalizarMantenimientoCommandHandler(
        IApplicationDbContext db,
        IValidator<FinalizarMantenimientoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(
        FinalizarMantenimientoCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Asignaciones
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Asignacion", command.Id);

        if (!entity.Activa)
        {
            throw new ConflictException("El mantenimiento no esta activo.");
        }

        var tipo = await _db.TiposAsignacion
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == entity.IdTipoAsignacion, cancellationToken);

        if (tipo is null || !TipoAsignacionNombres.EsNombre(tipo.Nombre, TipoAsignacionNombres.Mantenimiento))
        {
            throw new ConflictException("La asignacion no es un mantenimiento.");
        }

        var idEstadoAnterior = entity.IdEstado;
        var idEstadoRevertido = command.IdEstado
            ?? await LeerEstadoAnteriorDelHistorialAsync(entity.Id, cancellationToken)
            ?? entity.IdEstado;

        entity.Activa = false;
        entity.FechaDevolucion = command.FechaDevolucion is null || command.FechaDevolucion == default
            ? DateTime.UtcNow
            : command.FechaDevolucion;
        entity.IdEstado = idEstadoRevertido;

        var trabajo = command.Observaciones?.Trim() ?? string.Empty;

        _db.HistorialActivos.Add(new HistorialActivo
        {
            IdAsignacion = entity.Id,
            FechaHora = DateTime.UtcNow,
            TipoOperacion = "Mantenimiento",
            Descripcion = "Fin de mantenimiento",
            InformacionAnterior = $"id_estado={idEstadoAnterior}; activa=true; observaciones={entity.Observaciones}",
            InformacionNueva =
                $"id_estado={idEstadoRevertido}; activa=false; fecha_devolucion={entity.FechaDevolucion:o}; trabajo={trabajo}"
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<int?> LeerEstadoAnteriorDelHistorialAsync(int idAsignacion, CancellationToken cancellationToken)
    {
        var inicio = await _db.HistorialActivos
            .AsNoTracking()
            .Where(h => h.IdAsignacion == idAsignacion && h.Descripcion == "Inicio de mantenimiento")
            .OrderBy(h => h.FechaHora)
            .Select(h => h.InformacionAnterior)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(inicio))
        {
            return null;
        }

        const string prefix = "id_estado=";
        var token = inicio.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(p => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (token is null)
        {
            return null;
        }

        var raw = token[prefix.Length..];
        return int.TryParse(raw, out var parsed) && parsed > 0 ? parsed : null;
    }
}
