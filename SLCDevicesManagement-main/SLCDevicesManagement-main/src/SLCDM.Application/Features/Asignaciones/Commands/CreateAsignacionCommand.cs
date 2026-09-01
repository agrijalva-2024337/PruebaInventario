using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones.Commands;

/// <summary>
/// Registro de entrega (BE-15): quien entrega (<see cref="IdUsuario"/>),
/// quien recibe (<see cref="IdResponsable"/>), ubicacion de uso y firmas.
/// </summary>
public sealed record CreateAsignacionCommand(
    int IdActivo,
    int IdUsuario,
    int IdResponsable,
    int IdUbicacion,
    int IdEstado,
    int IdTipoAsignacion,
    DateTime FechaAsignacion,
    string? Observaciones,
    byte[]? FirmaEntrega,
    byte[]? FirmaRecibe,
    DateTime? FechaFirmaEntrega,
    string? DocumentoPdfUrl);

public sealed class CreateAsignacionCommandValidator : AbstractValidator<CreateAsignacionCommand>
{
    public CreateAsignacionCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdActivo)
            .RequiredId("id activo")
            .MustAsync(async (id, ct) => await db.Activos.AnyAsync(a => a.Id == id, ct))
            .WithMessage("No se encontro un activo con el id informado.");

        RuleFor(x => x.IdUsuario)
            .RequiredId("id usuario")
            .MustAsync(async (id, ct) => await db.Usuarios.AnyAsync(u => u.Id == id, ct))
            .WithMessage("No se encontro un usuario (quien entrega) con el id informado.");

        RuleFor(x => x.IdResponsable)
            .RequiredId("id responsable")
            .MustAsync(async (id, ct) => await db.Responsables.AnyAsync(r => r.Id == id, ct))
            .WithMessage("No se encontro un responsable (quien recibe) con el id informado.");

        RuleFor(x => x.IdUbicacion)
            .RequiredId("id ubicacion")
            .MustAsync(async (id, ct) => await db.Ubicaciones.AnyAsync(u => u.Id == id, ct))
            .WithMessage("No se encontro una ubicacion de uso con el id informado.");

        RuleFor(x => x.IdEstado)
            .RequiredId("id estado")
            .MustAsync(async (id, ct) => await db.Estados.AnyAsync(e => e.Id == id, ct))
            .WithMessage("No se encontro un estado con el id informado.");

        RuleFor(x => x.IdTipoAsignacion)
            .RequiredId("id tipo asignacion")
            .MustAsync(async (id, ct) => await db.TiposAsignacion.AnyAsync(t => t.Id == id, ct))
            .WithMessage("No se encontro un tipo de asignacion con el id informado.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await ActivoBajaRules.EstaDadoDeBajaAsync(db, cmd.IdActivo, ct))
            .WithMessage(ActivoBajaRules.MensajeActivoDadoDeBaja);

        // BE-14: solo el tipo "Asignacion" ocupa el activo; un activo, una activa.
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await ActivoTieneAsignacionActivaSiAplica(db, cmd, ct))
            .WithMessage("El activo ya tiene una asignacion activa. Un activo solo puede tener una asignacion activa a la vez.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        RuleFor(x => x.DocumentoPdfUrl)
            .MaximumLength(300).WithMessage("El campo documento pdf url no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentoPdfUrl));
    }

    private static async Task<bool> ActivoTieneAsignacionActivaSiAplica(
        IApplicationDbContext db,
        CreateAsignacionCommand command,
        CancellationToken cancellationToken)
    {
        var tipo = await db.TiposAsignacion
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == command.IdTipoAsignacion, cancellationToken);

        if (tipo is null || !TipoAsignacionNombres.EsTipoQueOcupaActivo(tipo.Nombre))
        {
            return false;
        }

        return await db.Asignaciones.AnyAsync(
            a => a.IdActivo == command.IdActivo && a.Activa,
            cancellationToken);
    }
}

public sealed class CreateAsignacionCommandHandler : ICommandHandler<CreateAsignacionCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateAsignacionCommand> _validator;

    public CreateAsignacionCommandHandler(IApplicationDbContext db, IValidator<CreateAsignacionCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateAsignacionCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var tipo = await _db.TiposAsignacion
            .AsNoTracking()
            .FirstAsync(t => t.Id == command.IdTipoAsignacion, cancellationToken);

        if (TipoAsignacionNombres.EsTipoQueOcupaActivo(tipo.Nombre)
            && await _db.Asignaciones.AnyAsync(a => a.IdActivo == command.IdActivo && a.Activa, cancellationToken))
        {
            throw new ConflictException(
                "El activo ya tiene una asignacion activa. Un activo solo puede tener una asignacion activa a la vez.");
        }

        if (await ActivoBajaRules.EstaDadoDeBajaAsync(_db, command.IdActivo, cancellationToken))
        {
            throw new ConflictException(ActivoBajaRules.MensajeActivoDadoDeBaja);
        }

        var entity = command.Adapt<Asignacion>();
        entity.Activa = true;
        if (entity.FechaAsignacion == default)
        {
            entity.FechaAsignacion = DateTime.UtcNow;
        }

        if (entity.FirmaEntrega is { Length: > 0 } || entity.FirmaRecibe is { Length: > 0 })
        {
            entity.FechaFirmaEntrega ??= DateTime.UtcNow;
        }

        _db.Asignaciones.Add(entity);

        var activo = await _db.Activos.FirstAsync(a => a.Id == command.IdActivo, cancellationToken);
        activo.IdUbicacion = command.IdUbicacion;

        await _db.SaveChangesAsync(cancellationToken);

        _db.HistorialActivos.Add(new HistorialActivo
        {
            IdAsignacion = entity.Id,
            FechaHora = DateTime.UtcNow,
            TipoOperacion = "Creacion",
            Descripcion = "Entrega de activo",
            InformacionNueva = $"Activo {entity.IdActivo} entregado a responsable {entity.IdResponsable} en ubicacion {entity.IdUbicacion}."
        });
        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
