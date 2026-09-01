using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Paises.Commands;

public sealed record DeletePaisCommand(int Id);

public sealed class DeletePaisCommandValidator : AbstractValidator<DeletePaisCommand>
{
    public DeletePaisCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id pais");
    }
}

public sealed class DeletePaisCommandHandler : ICommandHandler<DeletePaisCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DeletePaisCommand> _validator;

    public DeletePaisCommandHandler(IApplicationDbContext db, IValidator<DeletePaisCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DeletePaisCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Paises.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Pais", command.Id);

        var enUso = await _db.Sedes.AnyAsync(s => s.IdPais == command.Id, cancellationToken);
        if (enUso)
        {
            throw new ConflictException("No se puede eliminar el pais porque tiene sedes asociadas.");
        }

        _db.Paises.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
