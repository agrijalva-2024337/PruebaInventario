using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.ProductosCompra.Queries;

public sealed record GetProductoCompraByIdQuery(int Id);

public sealed class GetProductoCompraByIdQueryValidator : AbstractValidator<GetProductoCompraByIdQuery>
{
    public GetProductoCompraByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id producto compra");
    }
}

public sealed class GetProductoCompraByIdQueryHandler : IQueryHandler<GetProductoCompraByIdQuery, ProductoCompraDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetProductoCompraByIdQuery> _validator;

    public GetProductoCompraByIdQueryHandler(IApplicationDbContext db, IValidator<GetProductoCompraByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<ProductoCompraDto> HandleAsync(
        GetProductoCompraByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.ProductosCompra.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("ProductoCompra", query.Id);

        return entity.Adapt<ProductoCompraDto>();
    }
}
