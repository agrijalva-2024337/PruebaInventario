using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Estados.Queries;

public sealed record GetEstadosQuery;

public sealed class GetEstadosQueryHandler : IQueryHandler<GetEstadosQuery, IReadOnlyList<EstadoDto>>
{
    private readonly IApplicationDbContext _db;

    public GetEstadosQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<EstadoDto>> HandleAsync(
        GetEstadosQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = await _db.Estados
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<EstadoDto>>();
    }
}
