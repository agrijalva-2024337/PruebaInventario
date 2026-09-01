using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.DetallesActivos.Queries;

public sealed record GetDetallesActivosQuery(int? IdHistoricoInventario = null, int? IdActivo = null);

public sealed class GetDetallesActivosQueryHandler
    : IQueryHandler<GetDetallesActivosQuery, IReadOnlyList<DetalleActivoDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDetallesActivosQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DetalleActivoDto>> HandleAsync(
        GetDetallesActivosQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.DetallesActivos.AsNoTracking();

        if (query.IdHistoricoInventario.HasValue)
        {
            itemsQuery = itemsQuery.Where(d => d.IdHistoricoInventario == query.IdHistoricoInventario.Value);
        }

        if (query.IdActivo.HasValue)
        {
            itemsQuery = itemsQuery.Where(d => d.IdActivo == query.IdActivo.Value);
        }

        var items = await itemsQuery
            .OrderBy(d => d.Id)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<DetalleActivoDto>>();
    }
}
