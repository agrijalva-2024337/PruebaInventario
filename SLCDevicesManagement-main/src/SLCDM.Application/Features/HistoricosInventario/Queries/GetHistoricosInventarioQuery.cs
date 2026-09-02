using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.HistoricosInventario.Queries;

public sealed record GetHistoricosInventarioQuery(
    int? IdSede = null,
    int? IdEmpresa = null,
    bool? SoloAbiertos = null);

public sealed class GetHistoricosInventarioQueryHandler
    : IQueryHandler<GetHistoricosInventarioQuery, IReadOnlyList<HistoricoInventarioDto>>
{
    private readonly IApplicationDbContext _db;

    public GetHistoricosInventarioQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<HistoricoInventarioDto>> HandleAsync(
        GetHistoricosInventarioQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.HistoricosInventario.AsNoTracking();

        if (query.IdSede.HasValue)
        {
            itemsQuery = itemsQuery.Where(h => h.IdSede == query.IdSede.Value);
        }

        if (query.IdEmpresa.HasValue)
        {
            var idEmpresa = query.IdEmpresa.Value;
            itemsQuery = itemsQuery.Where(h =>
                _db.Sedes.Any(s => s.Id == h.IdSede && s.IdEmpresa == idEmpresa));
        }

        if (query.SoloAbiertos == true)
        {
            itemsQuery = itemsQuery.Where(h => !h.Cerrado);
        }
        else if (query.SoloAbiertos == false)
        {
            itemsQuery = itemsQuery.Where(h => h.Cerrado);
        }

        var items = await itemsQuery
            .OrderByDescending(h => h.FechaInicio)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<HistoricoInventarioDto>>();
    }
}
