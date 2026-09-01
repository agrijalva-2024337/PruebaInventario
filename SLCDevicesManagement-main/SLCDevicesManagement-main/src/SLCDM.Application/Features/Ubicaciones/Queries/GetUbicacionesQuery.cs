using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Ubicaciones.Queries;

public sealed record GetUbicacionesQuery(bool IncluirInhabilitados = false, int? IdSede = null);

public sealed class GetUbicacionesQueryHandler : IQueryHandler<GetUbicacionesQuery, IReadOnlyList<UbicacionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetUbicacionesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UbicacionDto>> HandleAsync(
        GetUbicacionesQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.Ubicaciones.AsNoTracking();

        if (!query.IncluirInhabilitados)
        {
            itemsQuery = itemsQuery.Where(u => u.Habilitado);
        }

        if (query.IdSede.HasValue)
        {
            itemsQuery = itemsQuery.Where(u => u.IdSede == query.IdSede.Value);
        }

        var items = await itemsQuery
            .OrderBy(u => u.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<UbicacionDto>>();
    }
}
