using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Dispositivos.Commands;

public sealed record RevocarDispositivoCommand(int Id);

public sealed class RevocarDispositivoCommandValidator : AbstractValidator<RevocarDispositivoCommand>
{
    public RevocarDispositivoCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id dispositivo token");
    }
}

public sealed class RevocarDispositivoCommandHandler : ICommandHandler<RevocarDispositivoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<RevocarDispositivoCommand> _validator;

    public RevocarDispositivoCommandHandler(IApplicationDbContext db, IValidator<RevocarDispositivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(RevocarDispositivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.DispositivosToken.FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("DispositivoToken", command.Id);

        entity.Revocado = true;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
