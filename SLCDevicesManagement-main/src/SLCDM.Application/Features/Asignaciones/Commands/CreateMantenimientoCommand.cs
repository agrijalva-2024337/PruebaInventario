using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones.Commands;

/// <summary>
/// Inicio de mantenimiento (BE-17): asignacion con tipo "Mantenimiento",
/// detalle en <c>observaciones</c> y <c>id_estado</c> operativo del proceso.
/// El estado anterior queda en <c>historial_activo</c> para revertirlo al finalizar.
/// </summary>
public sealed record CreateMantenimientoCommand(
    int IdActivo,
    int IdUsuario,
    int IdResponsable,
    int IdUbicacion,
    string Observaciones,
    DateTime FechaAsignacion,
    string? TipoMantenimiento = null,
    string? Costo = null,
    string? NumeroFactura = null,
    string? ProveedorTrabajo = null);

public sealed class CreateMantenimientoCommandValidator : AbstractValidator<CreateMantenimientoCommand>
{
    public CreateMantenimientoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdActivo)
            .RequiredId("id activo")
            .MustAsync(async (id, ct) => await db.Activos.AnyAsync(a => a.Id == id, ct))
            .WithMessage("No se encontro un activo con el id informado.");

        RuleFor(x => x.IdUsuario)
            .RequiredId("id usuario")
            .MustAsync(async (id, ct) => await db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Id == id, ct))
            .WithMessage("No se encontro un usuario con el id informado.");

        RuleFor(x => x.IdResponsable)
            .RequiredId("id responsable")
            .MustAsync(async (id, ct) => await db.Responsables.AnyAsync(r => r.Id == id, ct))
            .WithMessage("No se encontro un responsable con el id informado.");

        RuleFor(x => x.IdUbicacion)
            .RequiredId("id ubicacion")
            .MustAsync(async (id, ct) => await db.Ubicaciones.AnyAsync(u => u.Id == id, ct))
            .WithMessage("No se encontro una ubicacion con el id informado.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await ActivoBajaRules.EstaDadoDeBajaAsync(db, cmd.IdActivo, ct))
            .WithMessage(ActivoBajaRules.MensajeActivoDadoDeBaja);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                await AsignacionEmpresaRules.MismaEmpresaAsync(db, cmd.IdActivo, cmd.IdUbicacion, ct))
            .WithMessage("La ubicacion de mantenimiento debe pertenecer a la misma empresa del activo.");

        RuleFor(x => x.Observaciones)
            .NotEmpty().WithMessage("El campo observaciones es obligatorio.")
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.");
    }
}

public sealed class CreateMantenimientoCommandHandler : ICommandHandler<CreateMantenimientoCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateMantenimientoCommand> _validator;

    public CreateMantenimientoCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IValidator<CreateMantenimientoCommand> validator)
    {
        _db = db;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<int> HandleAsync(
        CreateMantenimientoCommand command,
        CancellationToken cancellationToken = default)
    {
        command = command with { IdUsuario = UsuarioSesion.IdQuienEntrega(_currentUser) };
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var tipo = await TipoAsignacionNombres.ObtenerRequeridoAsync(
            _db, TipoAsignacionNombres.Mantenimiento, cancellationToken);

        var existeActivo = await _db.Activos.AnyAsync(a => a.Id == command.IdActivo, cancellationToken);
        if (!existeActivo)
        {
            throw new NotFoundException("Activo", command.IdActivo);
        }

        if (await ActivoBajaRules.EstaDadoDeBajaAsync(_db, command.IdActivo, cancellationToken))
        {
            throw new ConflictException(ActivoBajaRules.MensajeActivoDadoDeBaja);
        }

        if (!await AsignacionEmpresaRules.MismaEmpresaAsync(
                _db, command.IdActivo, command.IdUbicacion, cancellationToken))
        {
            throw new ConflictException("La ubicacion de mantenimiento debe pertenecer a la misma empresa del activo.");
        }

        if (await ActivoTieneProcesoOcupandoAsync(command.IdActivo, cancellationToken))
        {
            throw new ConflictException(
                "El activo ya tiene una asignacion o un mantenimiento activo. Un activo solo puede tener un proceso ocupando a la vez.");
        }

        var estadoAnteriorId = await EstadoOperativoActualAsync(command.IdActivo, cancellationToken);
        var fecha = command.FechaAsignacion == default ? DateTime.UtcNow : command.FechaAsignacion;
        var estado = await EstadoNombres.ObtenerParaTipoAsync(_db, tipo.Nombre, cancellationToken);
        var observaciones = TiposMantenimiento.Prefijo(command.TipoMantenimiento);
        if (!string.IsNullOrWhiteSpace(command.ProveedorTrabajo))
        {
            observaciones += $"[Proveedor: {command.ProveedorTrabajo.Trim()}] ";
        }

        observaciones += command.Observaciones.Trim();
        if (observaciones.Length > 300)
        {
            observaciones = observaciones[..300];
        }

        var entity = new Asignacion
        {
            IdActivo = command.IdActivo,
            IdUsuario = command.IdUsuario,
            IdResponsable = command.IdResponsable,
            IdUbicacion = command.IdUbicacion,
            IdEstado = estado.Id,
            IdTipoAsignacion = tipo.Id,
            FechaAsignacion = fecha,
            Activa = true,
            Observaciones = observaciones
        };

        _db.Asignaciones.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var estadoAnteriorTexto = estadoAnteriorId.HasValue
            ? estadoAnteriorId.Value.ToString()
            : string.Empty;

        _db.HistorialActivos.Add(new HistorialActivo
        {
            IdAsignacion = entity.Id,
            FechaHora = DateTime.UtcNow,
            TipoOperacion = "Mantenimiento",
            Descripcion = "Inicio de mantenimiento",
            InformacionAnterior = $"id_estado={estadoAnteriorTexto}",
            InformacionNueva =
                $"id_estado={estado.Id}; tipo={command.TipoMantenimiento}; proveedor={command.ProveedorTrabajo}; costo={command.Costo}; factura={command.NumeroFactura}; observaciones={command.Observaciones}"
        });
        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private async Task<bool> ActivoTieneProcesoOcupandoAsync(int idActivo, CancellationToken cancellationToken)
    {
        var tipos = await _db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var idsOcupan = tipos
            .Where(t => TipoAsignacionNombres.EsTipoQueOcupaActivo(t.Nombre))
            .Select(t => t.Id)
            .ToList();

        if (idsOcupan.Count == 0)
        {
            return false;
        }

        return await _db.Asignaciones.AnyAsync(
            a => a.IdActivo == idActivo && a.Activa && idsOcupan.Contains(a.IdTipoAsignacion),
            cancellationToken);
    }

    private async Task<int?> EstadoOperativoActualAsync(int idActivo, CancellationToken cancellationToken)
    {
        return await _db.Asignaciones
            .AsNoTracking()
            .Where(a => a.IdActivo == idActivo && a.Activa)
            .OrderByDescending(a => a.FechaAsignacion)
            .Select(a => (int?)a.IdEstado)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
