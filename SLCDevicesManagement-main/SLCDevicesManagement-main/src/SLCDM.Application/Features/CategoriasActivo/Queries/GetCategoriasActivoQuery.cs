using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.CategoriasActivo.Queries;

public sealed record GetCategoriasActivoQuery(bool IncluirInhabilitados = false);

public sealed class GetCategoriasActivoQueryHandler
    : IQueryHandler<GetCategoriasActivoQuery, IReadOnlyList<CategoriaActivoDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCategoriasActivoQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CategoriaActivoDto>> HandleAsync(
        GetCategoriasActivoQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.CategoriasActivo.AsNoTracking();

        if (!query.IncluirInhabilitados)
        {
            itemsQuery = itemsQuery.Where(c => c.Habilitado);
        }

        var items = await itemsQuery
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<CategoriaActivoDto>>();
    }
}
