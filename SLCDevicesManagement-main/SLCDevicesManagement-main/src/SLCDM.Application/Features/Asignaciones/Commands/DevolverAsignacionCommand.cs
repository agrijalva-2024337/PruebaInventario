using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones.Commands;

public sealed record DevolverAsignacionCommand(
    int Id,
    DateTime? FechaDevolucion,
    string? Observaciones);

public sealed class DevolverAsignacionCommandValidator : AbstractValidator<DevolverAsignacionCommand>
{
    public DevolverAsignacionCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id asignacion");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}

public sealed class DevolverAsignacionCommandHandler : ICommandHandler<DevolverAsignacionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DevolverAsignacionCommand> _validator;

    public DevolverAsignacionCommandHandler(IApplicationDbContext db, IValidator<DevolverAsignacionCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DevolverAsignacionCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Asignaciones.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Asignacion", command.Id);

        if (!entity.Activa)
        {
            throw new ConflictException("La asignacion no esta activa.");
        }

        var informacionAnterior = $"activa=true; fecha_devolucion=";

        entity.Activa = false;
        entity.FechaDevolucion = command.FechaDevolucion is null || command.FechaDevolucion == default
            ? DateTime.UtcNow
            : command.FechaDevolucion;

        if (command.Observaciones is not null)
        {
            entity.Observaciones = command.Observaciones;
        }

        _db.HistorialActivos.Add(new HistorialActivo
        {
            IdAsignacion = entity.Id,
            FechaHora = DateTime.UtcNow,
            TipoOperacion = "Modificacion",
            Descripcion = "Devolucion de activo",
            InformacionAnterior = informacionAnterior,
            InformacionNueva = $"activa=false; fecha_devolucion={entity.FechaDevolucion:o}"
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
