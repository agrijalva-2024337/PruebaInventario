using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.RedesConocidas.Commands;

public sealed record DeleteRedConocidaCommand(int Id);

public sealed class DeleteRedConocidaCommandValidator : AbstractValidator<DeleteRedConocidaCommand>
{
    public DeleteRedConocidaCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id red conocida");
    }
}

public sealed class DeleteRedConocidaCommandHandler : ICommandHandler<DeleteRedConocidaCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DeleteRedConocidaCommand> _validator;

    public DeleteRedConocidaCommandHandler(IApplicationDbContext db, IValidator<DeleteRedConocidaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DeleteRedConocidaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.RedesConocidas.FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("RedConocida", command.Id);

        _db.RedesConocidas.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
