using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Paises.Queries;

public sealed record GetPaisesQuery;

public sealed class GetPaisesQueryHandler : IQueryHandler<GetPaisesQuery, IReadOnlyList<PaisDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPaisesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PaisDto>> HandleAsync(
        GetPaisesQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = await _db.Paises
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<PaisDto>>();
    }
}
