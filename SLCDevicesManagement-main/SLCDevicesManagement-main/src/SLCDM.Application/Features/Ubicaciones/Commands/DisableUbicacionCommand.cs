using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Ubicaciones.Commands;

public sealed record DisableUbicacionCommand(int Id);

public sealed class DisableUbicacionCommandValidator : AbstractValidator<DisableUbicacionCommand>
{
    public DisableUbicacionCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id ubicacion");
    }
}

public sealed class DisableUbicacionCommandHandler : ICommandHandler<DisableUbicacionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DisableUbicacionCommand> _validator;

    public DisableUbicacionCommandHandler(IApplicationDbContext db, IValidator<DisableUbicacionCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DisableUbicacionCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Ubicaciones.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Ubicacion", command.Id);

        entity.Habilitado = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
