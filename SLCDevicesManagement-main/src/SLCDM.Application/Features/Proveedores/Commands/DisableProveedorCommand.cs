using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Proveedores.Commands;

public sealed record DisableProveedorCommand(int Id);

public sealed class DisableProveedorCommandValidator : AbstractValidator<DisableProveedorCommand>
{
    public DisableProveedorCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id proveedor");
    }
}

public sealed class DisableProveedorCommandHandler : ICommandHandler<DisableProveedorCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DisableProveedorCommand> _validator;

    public DisableProveedorCommandHandler(IApplicationDbContext db, IValidator<DisableProveedorCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DisableProveedorCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Proveedores.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Proveedor", command.Id);

        entity.Habilitado = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
