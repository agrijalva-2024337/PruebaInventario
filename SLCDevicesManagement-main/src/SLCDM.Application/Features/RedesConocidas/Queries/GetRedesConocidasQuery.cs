using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.RedesConocidas.Queries;

public sealed record GetRedesConocidasQuery(int? IdUbicacion = null);

public sealed class GetRedesConocidasQueryHandler : IQueryHandler<GetRedesConocidasQuery, IReadOnlyList<RedConocidaDto>>
{
    private readonly IApplicationDbContext _db;

    public GetRedesConocidasQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RedConocidaDto>> HandleAsync(
        GetRedesConocidasQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.RedesConocidas.AsNoTracking();

        if (query.IdUbicacion.HasValue)
        {
            itemsQuery = itemsQuery.Where(r => r.IdUbicacion == query.IdUbicacion.Value);
        }

        var items = await itemsQuery
            .OrderBy(r => r.Bssid)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<RedConocidaDto>>();
    }
}
