using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.CategoriasActivo.Commands;

public sealed record DisableCategoriaActivoCommand(int Id);

public sealed class DisableCategoriaActivoCommandValidator : AbstractValidator<DisableCategoriaActivoCommand>
{
    public DisableCategoriaActivoCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id categoria activo");
    }
}

public sealed class DisableCategoriaActivoCommandHandler : ICommandHandler<DisableCategoriaActivoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DisableCategoriaActivoCommand> _validator;

    public DisableCategoriaActivoCommandHandler(
        IApplicationDbContext db,
        IValidator<DisableCategoriaActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(
        DisableCategoriaActivoCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.CategoriasActivo
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("CategoriaActivo", command.Id);

        entity.Habilitado = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
