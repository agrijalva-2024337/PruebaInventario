using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.TiposAsignacion.Queries;

public sealed record GetTiposAsignacionQuery;

public sealed class GetTiposAsignacionQueryHandler : IQueryHandler<GetTiposAsignacionQuery, IReadOnlyList<TipoAsignacionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTiposAsignacionQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TipoAsignacionDto>> HandleAsync(
        GetTiposAsignacionQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = await _db.TiposAsignacion
            .AsNoTracking()
            .OrderBy(t => t.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<TipoAsignacionDto>>();
    }
}
