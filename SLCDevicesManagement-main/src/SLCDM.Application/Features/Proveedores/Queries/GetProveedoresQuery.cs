using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Proveedores.Queries;

public sealed record GetProveedoresQuery(bool IncluirInhabilitados = false, int? IdEmpresa = null);

public sealed class GetProveedoresQueryHandler : IQueryHandler<GetProveedoresQuery, IReadOnlyList<ProveedorDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProveedoresQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProveedorDto>> HandleAsync(
        GetProveedoresQuery query,
        CancellationToken cancellationToken = default)
    {
        var itemsQuery = _db.Proveedores.AsNoTracking();

        if (!query.IncluirInhabilitados)
        {
            itemsQuery = itemsQuery.Where(p => p.Habilitado);
        }

        if (query.IdEmpresa.HasValue)
        {
            itemsQuery = itemsQuery.Where(p => p.IdEmpresa == query.IdEmpresa.Value);
        }

        var items = await itemsQuery
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<ProveedorDto>>();
    }
}
