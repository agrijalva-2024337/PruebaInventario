using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.ProductosCompra.Queries;

public sealed record GetProductosCompraQuery(bool IncluirInhabilitados = false, int? IdProveedor = null);

public sealed class GetProductosCompraQueryHandler
    : IQueryHandler<GetProductosCompraQuery, IReadOnlyList<ProductoCompraDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProductosCompraQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProductoCompraDto>> HandleAsync(
        GetProductosCompraQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.ProductosCompra.AsNoTracking();

        if (!query.IncluirInhabilitados)
        {
            itemsQuery = itemsQuery.Where(p => p.Habilitado);
        }

        if (query.IdProveedor.HasValue)
        {
            itemsQuery = itemsQuery.Where(p => p.IdProveedor == query.IdProveedor.Value);
        }

        var items = await itemsQuery
            .OrderBy(p => p.Nombre)
            .ThenByDescending(p => p.FechaCompra)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<ProductoCompraDto>>();
    }
}
