using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Asignaciones.Queries;

public sealed record GetAsignacionesQuery(
    int? IdActivo = null,
    int? IdUsuario = null,
    bool? SoloActivas = null);

public sealed class GetAsignacionesQueryHandler : IQueryHandler<GetAsignacionesQuery, IReadOnlyList<AsignacionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAsignacionesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AsignacionDto>> HandleAsync(
        GetAsignacionesQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.Asignaciones.AsNoTracking();

        if (query.IdActivo.HasValue)
        {
            itemsQuery = itemsQuery.Where(a => a.IdActivo == query.IdActivo.Value);
        }

        if (query.IdUsuario.HasValue)
        {
            itemsQuery = itemsQuery.Where(a => a.IdUsuario == query.IdUsuario.Value);
        }

        if (query.SoloActivas.HasValue)
        {
            itemsQuery = itemsQuery.Where(a => a.Activa == query.SoloActivas.Value);
        }

        var items = await itemsQuery
            .OrderByDescending(a => a.FechaAsignacion)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<AsignacionDto>>();
    }
}
