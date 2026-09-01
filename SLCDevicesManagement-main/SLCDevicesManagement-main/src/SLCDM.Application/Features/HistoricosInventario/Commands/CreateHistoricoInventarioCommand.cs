using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.HistoricosInventario.Commands;

/// <summary>
/// Apertura de jornada de inventario fisico (BE-20) por sede.
/// No clona activos: el operador registra hallazgos en <c>Detalle_Activo</c>.
/// El universo teorico al cierre son los activos cuya <c>Ubicacion.IdSede</c>
/// coincide con la jornada y que no estan dados de baja
/// (<see cref="Asignaciones.ActivoBajaRules.EstaDadoDeBajaAsync"/>).
/// </summary>
public sealed record CreateHistoricoInventarioCommand(
    int IdSede,
    string? Responsable,
    DateTime FechaInicio,
    string? Observaciones);

public sealed class CreateHistoricoInventarioCommandValidator : AbstractValidator<CreateHistoricoInventarioCommand>
{
    public CreateHistoricoInventarioCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdSede)
            .RequiredId("id sede")
            .MustAsync(async (id, ct) => await db.Sedes.AnyAsync(s => s.Id == id, ct))
            .WithMessage("No se encontro una sede con el id informado.")
            .MustAsync(async (id, ct) =>
                !await db.HistoricosInventario.AnyAsync(h => h.IdSede == id && !h.Cerrado, ct))
            .WithMessage("Ya existe una jornada de inventario abierta para esta sede.");

        RuleFor(x => x.Responsable)
            .MaximumLength(150).WithMessage("El campo responsable no debe superar los 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Responsable));

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}

public sealed class CreateHistoricoInventarioCommandHandler : ICommandHandler<CreateHistoricoInventarioCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateHistoricoInventarioCommand> _validator;

    public CreateHistoricoInventarioCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IValidator<CreateHistoricoInventarioCommand> validator)
    {
        _db = db;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<int> HandleAsync(
        CreateHistoricoInventarioCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var sede = await _db.Sedes.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.IdSede, cancellationToken)
            ?? throw new NotFoundException("Sede", command.IdSede);

        if (!_currentUser.IsAdministradorGeneral
            && (!_currentUser.EmpresaId.HasValue || sede.IdEmpresa != _currentUser.EmpresaId.Value))
        {
            throw new ConflictException("La sede no pertenece a la empresa del usuario.");
        }

        if (await _db.HistoricosInventario.AnyAsync(h => h.IdSede == command.IdSede && !h.Cerrado, cancellationToken))
        {
            throw new ConflictException("Ya existe una jornada de inventario abierta para esta sede.");
        }

        var entity = command.Adapt<HistoricoInventario>();
        entity.Cerrado = false;
        if (entity.FechaInicio == default)
        {
            entity.FechaInicio = DateTime.UtcNow;
        }

        _db.HistoricosInventario.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
