using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Areas.Commands;

public sealed record DisableAreaCommand(int Id);

public sealed class DisableAreaCommandValidator : AbstractValidator<DisableAreaCommand>
{
    public DisableAreaCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id area");
    }
}

public sealed class DisableAreaCommandHandler : ICommandHandler<DisableAreaCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DisableAreaCommand> _validator;

    public DisableAreaCommandHandler(IApplicationDbContext db, IValidator<DisableAreaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DisableAreaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Areas.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Area", command.Id);

        entity.Habilitado = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
