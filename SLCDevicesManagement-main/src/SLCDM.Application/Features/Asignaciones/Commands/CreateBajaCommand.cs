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
    string Motivo,
    string? DocumentoPdfUrl,
    DateTime FechaAsignacion,
    byte[]? FirmaEntrega,
    byte[]? FirmaRecibe);

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

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El campo motivo es obligatorio.")
            .MaximumLength(300).WithMessage("El campo motivo no debe superar los 300 caracteres.")
            .Must(MotivosBaja.EsCanonico)
            .WithMessage("El motivo de baja debe ser: Venta, Desecho, Donacion, Perdida, Robo, Dano irreparable u Otro.");

        RuleFor(x => x.DocumentoPdfUrl)
            .MaximumLength(300).WithMessage("El campo documento de referencia no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentoPdfUrl));

        RuleFor(x => x.FirmaEntrega)
            .Must(AsignacionDocumento.TieneTinta)
            .WithMessage("La firma de quien entrega el activo es obligatoria.");

        RuleFor(x => x.FirmaRecibe)
            .Must(AsignacionDocumento.TieneTinta)
            .WithMessage("La firma de quien autoriza la baja es obligatoria.");
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

        if (await ActivoEnMantenimientoActivoAsync(command.IdActivo, cancellationToken))
        {
            throw new ConflictException(
                "El activo esta en mantenimiento. Finalice el mantenimiento antes de darlo de baja.");
        }

        var asignacionesActivas = await CargarAsignacionesActivasAsync(command.IdActivo, cancellationToken);
        var fecha = command.FechaAsignacion == default ? DateTime.UtcNow : command.FechaAsignacion;
        foreach (var asignacionActiva in asignacionesActivas)
        {
            asignacionActiva.Activa = false;
            asignacionActiva.FechaDevolucion = fecha;
        }
        var estado = await EstadoNombres.ObtenerParaTipoAsync(_db, tipo.Nombre, cancellationToken);

        var entity = new Asignacion
        {
            IdActivo = command.IdActivo,
            IdUsuario = command.IdUsuario,
            IdResponsable = command.IdResponsable,
            IdUbicacion = activo.IdUbicacion,
            IdEstado = estado.Id,
            IdTipoAsignacion = tipo.Id,
            FechaAsignacion = fecha,
            Activa = true,
            Observaciones = command.Motivo,
            FirmaEntrega = command.FirmaEntrega,
            FirmaRecibe = command.FirmaRecibe,
            FechaFirmaEntrega = DateTime.UtcNow
        };

        _db.Asignaciones.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        entity.DocumentoPdfUrl = string.IsNullOrWhiteSpace(command.DocumentoPdfUrl)
            ? AsignacionDocumento.PdfRelativeUrl(entity.Id)
            : command.DocumentoPdfUrl.Trim();
        entity.DocumentoPdfGenerardoEn = DateTime.UtcNow;

        _db.HistorialActivos.Add(new HistorialActivo
        {
            IdAsignacion = entity.Id,
            FechaHora = DateTime.UtcNow,
            TipoOperacion = "Baja",
            Descripcion = "Baja de activo",
            InformacionAnterior = $"id_activo={command.IdActivo}; asignaciones_cerradas={asignacionesActivas.Count}",
            InformacionNueva =
                $"motivo={command.Motivo}; documento_pdf_url={command.DocumentoPdfUrl}; id_responsable={command.IdResponsable}"
        });
        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private async Task<List<Asignacion>> CargarAsignacionesActivasAsync(
        int idActivo,
        CancellationToken cancellationToken)
    {
        var tipos = await _db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var idsAsignacion = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Asignacion))
            .Select(t => t.Id)
            .ToList();

        if (idsAsignacion.Count == 0)
        {
            return [];
        }

        return await _db.Asignaciones
            .Where(a => a.IdActivo == idActivo && a.Activa && idsAsignacion.Contains(a.IdTipoAsignacion))
            .ToListAsync(cancellationToken);
    }

    private async Task<bool> ActivoEnMantenimientoActivoAsync(int idActivo, CancellationToken cancellationToken)
    {
        var tipos = await _db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var idsMantenimiento = tipos
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
