using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones.Commands;

/// <summary>
/// Traslado interno (BE-16): asignacion con tipo "Traslado",
/// actualiza <c>activo.id_ubicacion</c> y deja rastro en <c>historial_activo</c>.
/// No se permiten traslados entre empresas distintas.
/// </summary>
public sealed record CreateTrasladoCommand(
    int IdActivo,
    int IdUsuario,
    int IdResponsable,
    int IdUbicacion,
    DateTime FechaAsignacion,
    string? Observaciones);

public sealed class CreateTrasladoCommandValidator : AbstractValidator<CreateTrasladoCommand>
{
    public CreateTrasladoCommandValidator(IApplicationDbContext db)
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
            .WithMessage("No se encontro una ubicacion destino con el id informado.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await ActivoBajaRules.EstaDadoDeBajaAsync(db, cmd.IdActivo, ct))
            .WithMessage(ActivoBajaRules.MensajeActivoDadoDeBaja);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                await AsignacionEmpresaRules.MismaEmpresaAsync(db, cmd.IdActivo, cmd.IdUbicacion, ct))
            .WithMessage("No se permiten traslados entre empresas distintas. La ubicacion destino debe pertenecer a la misma empresa del activo.");

        RuleFor(x => x.Observaciones)
            .NotEmpty().WithMessage("El motivo del traslado es obligatorio.")
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.");
    }
}

public sealed class CreateTrasladoCommandHandler : ICommandHandler<CreateTrasladoCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateTrasladoCommand> _validator;

    public CreateTrasladoCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IValidator<CreateTrasladoCommand> validator)
    {
        _db = db;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateTrasladoCommand command, CancellationToken cancellationToken = default)
    {
        command = command with { IdUsuario = UsuarioSesion.IdQuienEntrega(_currentUser) };
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var tipo = await TipoAsignacionNombres.ObtenerRequeridoAsync(
            _db, TipoAsignacionNombres.Traslado, cancellationToken);

        var activo = await _db.Activos.FirstOrDefaultAsync(a => a.Id == command.IdActivo, cancellationToken)
            ?? throw new NotFoundException("Activo", command.IdActivo);

        if (await ActivoBajaRules.EstaDadoDeBajaAsync(_db, command.IdActivo, cancellationToken))
        {
            throw new ConflictException(ActivoBajaRules.MensajeActivoDadoDeBaja);
        }

        if (activo.IdUbicacion == command.IdUbicacion)
        {
            throw new ConflictException("La ubicacion destino es la misma que la actual del activo.");
        }

        if (!await AsignacionEmpresaRules.MismaEmpresaAsync(
                _db, command.IdActivo, command.IdUbicacion, cancellationToken))
        {
            throw new ConflictException(
                "No se permiten traslados entre empresas distintas. La ubicacion destino debe pertenecer a la misma empresa del activo.");
        }

        var enMantenimiento = await ActivoEnMantenimientoActivoAsync(command.IdActivo, cancellationToken);
        if (enMantenimiento)
        {
            throw new ConflictException("El activo esta en mantenimiento. Finalice el mantenimiento antes de trasladarlo.");
        }

        var idUbicacionAnterior = activo.IdUbicacion;
        var fecha = command.FechaAsignacion == default ? DateTime.UtcNow : command.FechaAsignacion;
        var estado = await EstadoNombres.ObtenerParaTipoAsync(_db, tipo.Nombre, cancellationToken);

        var entity = new Asignacion
        {
            IdActivo = command.IdActivo,
            IdUsuario = command.IdUsuario,
            IdResponsable = command.IdResponsable,
            IdUbicacion = command.IdUbicacion,
            IdEstado = estado.Id,
            IdTipoAsignacion = tipo.Id,
            FechaAsignacion = fecha,
            Activa = false,
            FechaDevolucion = fecha,
            Observaciones = command.Observaciones
        };

        _db.Asignaciones.Add(entity);
        activo.IdUbicacion = command.IdUbicacion;
        await _db.SaveChangesAsync(cancellationToken);

        _db.HistorialActivos.Add(new HistorialActivo
        {
            IdAsignacion = entity.Id,
            FechaHora = DateTime.UtcNow,
            TipoOperacion = "Traslado",
            Descripcion = "Traslado de activo",
            InformacionAnterior = $"id_ubicacion={idUbicacionAnterior}",
            InformacionNueva =
                $"id_ubicacion={command.IdUbicacion}; id_activo={command.IdActivo}; motivo={command.Observaciones}; id_responsable={command.IdResponsable}"
        });
        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private async Task<bool> ActivoEnMantenimientoActivoAsync(int idActivo, CancellationToken cancellationToken)
    {
        var tipoIds = await _db.TiposAsignacion
            .AsNoTracking()
            .Select(t => new { t.Id, t.Nombre })
            .ToListAsync(cancellationToken);

        var idsMantenimiento = tipoIds
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Mantenimiento))
            .Select(t => t.Id)
            .ToList();

        if (idsMantenimiento.Count == 0)
        {
            return false;
        }

        return await _db.Asignaciones.AnyAsync(
            a => a.IdActivo == idActivo && a.Activa && idsMantenimiento.Contains(a.IdTipoAsignacion),
            cancellationToken);
    }
}
