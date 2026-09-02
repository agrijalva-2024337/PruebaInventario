using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.HistoricosInventario.Commands;

/// <summary>
/// Cierre de jornada (BE-20). Irreversible: no hay reapertura.
/// Tras el cierre, Create/Update/Delete de DetalleActivo responden 409.
/// </summary>
public sealed record CerrarHistoricoInventarioCommand(int Id, string? Observaciones);

public sealed class CerrarHistoricoInventarioCommandValidator : AbstractValidator<CerrarHistoricoInventarioCommand>
{
    public CerrarHistoricoInventarioCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id historico inventario");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}

public sealed class CerrarHistoricoInventarioCommandHandler : ICommandHandler<CerrarHistoricoInventarioCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CerrarHistoricoInventarioCommand> _validator;

    public CerrarHistoricoInventarioCommandHandler(
        IApplicationDbContext db,
        IValidator<CerrarHistoricoInventarioCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(
        CerrarHistoricoInventarioCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.HistoricosInventario
            .FirstOrDefaultAsync(h => h.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("HistoricoInventario", command.Id);

        if (entity.Cerrado)
        {
            throw new ConflictException("El historico de inventario ya esta cerrado.");
        }

        entity.Cerrado = true;
        entity.FechaCierre = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(command.Observaciones))
        {
            entity.Observaciones = command.Observaciones;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
