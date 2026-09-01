using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones.Commands;

/// <summary>
/// Baja de activo (BE-18): asignacion con tipo "Baja". No elimina el registro.
/// Requiere motivo, documento de referencia y responsable que autoriza.
/// </summary>
public sealed record CreateBajaCommand(
    int IdActivo,
    int IdUsuario,
    int IdResponsable,
    int IdEstado,
    string Motivo,
    string DocumentoPdfUrl,
    DateTime FechaAsignacion);

public sealed class CreateBajaCommandValidator : AbstractValidator<CreateBajaCommand>
{
    public CreateBajaCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdActivo)
            .RequiredId("id activo")
            .MustAsync(async (id, ct) => await db.Activos.AnyAsync(a => a.Id == id, ct))
            .WithMessage("No se encontro un activo con el id informado.");

        RuleFor(x => x.IdUsuario)
            .RequiredId("id usuario")
            .MustAsync(async (id, ct) => await db.Usuarios.AnyAsync(u => u.Id == id, ct))
            .WithMessage("No se encontro un usuario con el id informado.");

        RuleFor(x => x.IdResponsable)
            .RequiredId("id responsable")
            .MustAsync(async (id, ct) => await db.Responsables.AnyAsync(r => r.Id == id, ct))
            .WithMessage("No se encontro un responsable autorizador con el id informado.");

        RuleFor(x => x.IdEstado)
            .RequiredId("id estado")
            .MustAsync(async (id, ct) => await db.Estados.AnyAsync(e => e.Id == id, ct))
            .WithMessage("No se encontro un estado con el id informado.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El campo motivo es obligatorio.")
            .MaximumLength(300).WithMessage("El campo motivo no debe superar los 300 caracteres.");

        RuleFor(x => x.DocumentoPdfUrl)
            .NotEmpty().WithMessage("El campo documento pdf url es obligatorio.")
            .MaximumLength(300).WithMessage("El campo documento pdf url no debe superar los 300 caracteres.");
    }
}

public sealed class CreateBajaCommandHandler : ICommandHandler<CreateBajaCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateBajaCommand> _validator;

    public CreateBajaCommandHandler(IApplicationDbContext db, IValidator<CreateBajaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateBajaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var tipo = await TipoAsignacionNombres.ObtenerRequeridoAsync(
            _db, TipoAsignacionNombres.Baja, cancellationToken);

        var activo = await _db.Activos.FirstOrDefaultAsync(a => a.Id == command.IdActivo, cancellationToken)
            ?? throw new NotFoundException("Activo", command.IdActivo);

        if (await ActivoBajaRules.EstaDadoDeBajaAsync(_db, command.IdActivo, cancellationToken))
        {
            throw new ConflictException("El activo ya esta dado de baja.");
        }

        if (await ActivoTieneProcesoOcupandoAsync(command.IdActivo, cancellationToken))
        {
            throw new ConflictException(
                "El activo tiene una asignacion o un mantenimiento activo. Cierren el proceso antes de dar de baja.");
        }

        var fecha = command.FechaAsignacion == default ? DateTime.UtcNow : command.FechaAsignacion;

        var entity = new Asignacion
        {
            IdActivo = command.IdActivo,
            IdUsuario = command.IdUsuario,
            IdResponsable = command.IdResponsable,
            IdUbicacion = activo.IdUbicacion,
            IdEstado = command.IdEstado,
            IdTipoAsignacion = tipo.Id,
            FechaAsignacion = fecha,
            Activa = true,
            Observaciones = command.Motivo,
            DocumentoPdfUrl = command.DocumentoPdfUrl
        };

        _db.Asignaciones.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _db.HistorialActivos.Add(new HistorialActivo
        {
            IdAsignacion = entity.Id,
            FechaHora = DateTime.UtcNow,
            TipoOperacion = "Baja",
            Descripcion = "Baja de activo",
            InformacionAnterior = $"id_activo={command.IdActivo}",
            InformacionNueva =
                $"motivo={command.Motivo}; documento_pdf_url={command.DocumentoPdfUrl}; id_responsable={command.IdResponsable}"
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
}
