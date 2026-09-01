using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.HistorialActivos.Queries;

public sealed record GetHistorialActivosQuery(int? IdAsignacion = null, int? IdDetalleActivo = null);

public sealed class GetHistorialActivosQueryHandler
    : IQueryHandler<GetHistorialActivosQuery, IReadOnlyList<HistorialActivoDto>>
{
    private readonly IApplicationDbContext _db;

    public GetHistorialActivosQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<HistorialActivoDto>> HandleAsync(
        GetHistorialActivosQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.HistorialActivos.AsNoTracking();

        if (query.IdAsignacion.HasValue)
        {
            itemsQuery = itemsQuery.Where(h => h.IdAsignacion == query.IdAsignacion.Value);
        }

        if (query.IdDetalleActivo.HasValue)
        {
            itemsQuery = itemsQuery.Where(h => h.IdDetalleActivo == query.IdDetalleActivo.Value);
        }

        var items = await itemsQuery
            .OrderByDescending(h => h.FechaHora)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<HistorialActivoDto>>();
    }
}
