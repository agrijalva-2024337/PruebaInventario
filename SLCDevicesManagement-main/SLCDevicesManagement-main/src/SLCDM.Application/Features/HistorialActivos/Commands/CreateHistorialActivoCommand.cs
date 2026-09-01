using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.HistorialActivos.Commands;

public sealed record CreateHistorialActivoCommand(
    int? IdAsignacion,
    int? IdDetalleActivo,
    string? TipoOperacion,
    string? Descripcion,
    string? InformacionAnterior,
    string? InformacionNueva);

public sealed class CreateHistorialActivoCommandValidator : AbstractValidator<CreateHistorialActivoCommand>
{
    public CreateHistorialActivoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x)
            .Must(x => x.IdAsignacion.HasValue || x.IdDetalleActivo.HasValue)
            .WithMessage("Debe informar al menos id asignacion o id detalle activo.");

        RuleFor(x => x.IdAsignacion)
            .OptionalId("id asignacion")
            .MustAsync(async (id, ct) =>
                !id.HasValue || await db.Asignaciones.AnyAsync(a => a.Id == id.Value, ct))
            .WithMessage("No se encontro una asignacion con el id informado.");

        RuleFor(x => x.IdDetalleActivo)
            .OptionalId("id detalle activo")
            .MustAsync(async (id, ct) =>
                !id.HasValue || await db.DetallesActivos.AnyAsync(d => d.Id == id.Value, ct))
            .WithMessage("No se encontro un detalle de activo con el id informado.");

        RuleFor(x => x.TipoOperacion)
            .MaximumLength(30).WithMessage("El campo tipo operacion no debe superar los 30 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.TipoOperacion));

        RuleFor(x => x.Descripcion)
            .MaximumLength(300).WithMessage("El campo descripcion no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class CreateHistorialActivoCommandHandler : ICommandHandler<CreateHistorialActivoCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateHistorialActivoCommand> _validator;

    public CreateHistorialActivoCommandHandler(
        IApplicationDbContext db,
        IValidator<CreateHistorialActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(
        CreateHistorialActivoCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<HistorialActivo>();
        entity.FechaHora = DateTime.UtcNow;

        _db.HistorialActivos.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
