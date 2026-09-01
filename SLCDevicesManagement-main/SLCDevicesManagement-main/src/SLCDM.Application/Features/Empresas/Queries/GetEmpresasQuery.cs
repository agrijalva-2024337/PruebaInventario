using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Empresas.Queries;

public sealed record GetEmpresasQuery(bool IncluirInhabilitados = false);

public sealed class GetEmpresasQueryHandler : IQueryHandler<GetEmpresasQuery, IReadOnlyList<EmpresaDto>>
{
    private readonly IApplicationDbContext _db;

    public GetEmpresasQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<EmpresaDto>> HandleAsync(
        GetEmpresasQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Empresas.AsNoTracking();
        if (!query.IncluirInhabilitados)
        {
            q = q.Where(e => e.Habilitado);
        }

        var items = await q
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<EmpresaDto>>();
    }
}
