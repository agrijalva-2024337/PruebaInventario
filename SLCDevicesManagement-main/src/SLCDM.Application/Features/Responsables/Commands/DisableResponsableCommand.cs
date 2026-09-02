using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Responsables.Commands;

public sealed record DisableResponsableCommand(int Id);

public sealed class DisableResponsableCommandValidator : AbstractValidator<DisableResponsableCommand>
{
    public DisableResponsableCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id responsable");
    }
}

public sealed class DisableResponsableCommandHandler : ICommandHandler<DisableResponsableCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DisableResponsableCommand> _validator;

    public DisableResponsableCommandHandler(IApplicationDbContext db, IValidator<DisableResponsableCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DisableResponsableCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Responsables.FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Responsable", command.Id);

        entity.Habilitado = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
