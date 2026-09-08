using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.ProductosCompra.Commands;

public sealed record DisableProductoCompraCommand(int Id);

public sealed class DisableProductoCompraCommandValidator : AbstractValidator<DisableProductoCompraCommand>
{
    public DisableProductoCompraCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id producto compra");
    }
}

public sealed class DisableProductoCompraCommandHandler : ICommandHandler<DisableProductoCompraCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<DisableProductoCompraCommand> _validator;

    public DisableProductoCompraCommandHandler(IApplicationDbContext db, IValidator<DisableProductoCompraCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(DisableProductoCompraCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.ProductosCompra.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("ProductoCompra", command.Id);

        entity.Habilitado = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
