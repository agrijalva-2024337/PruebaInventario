using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Empresas.Commands;

public sealed record DisableEmpresaCommand(int Id);

public sealed class DisableEmpresaCommandValidator : AbstractValidator<DisableEmpresaCommand>
{
    public DisableEmpresaCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id empresa");
    }
}

public sealed class DisableEmpresaCommandHandler : ICommandHandler<DisableEmpresaCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DisableEmpresaCommand> _validator;

    public DisableEmpresaCommandHandler(IApplicationDbContext db, IValidator<DisableEmpresaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DisableEmpresaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Empresas.FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Empresa", command.Id);

        entity.Habilitado = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
