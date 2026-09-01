using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Activos.Queries;

public sealed record GetActivosQuery(
    int? IdCategoriaActivo = null,
    int? IdProveedor = null,
    int? IdUbicacion = null);

public sealed class GetActivosQueryHandler : IQueryHandler<GetActivosQuery, IReadOnlyList<ActivoDto>>
{
    private readonly IApplicationDbContext _db;

    public GetActivosQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ActivoDto>> HandleAsync(
        GetActivosQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.Activos.AsNoTracking();

        if (query.IdCategoriaActivo.HasValue)
        {
            itemsQuery = itemsQuery.Where(a => a.IdCategoriaActivo == query.IdCategoriaActivo.Value);
        }

        if (query.IdProveedor.HasValue)
        {
            itemsQuery = itemsQuery.Where(a => a.IdProveedor == query.IdProveedor.Value);
        }

        if (query.IdUbicacion.HasValue)
        {
            itemsQuery = itemsQuery.Where(a => a.IdUbicacion == query.IdUbicacion.Value);
        }

        var items = await itemsQuery
            .OrderBy(a => a.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<ActivoDto>>();
    }
}
