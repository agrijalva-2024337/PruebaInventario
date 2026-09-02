using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Reportes.Queries;

public sealed record GetActivosPorResponsableQuery(int? IdEmpresa = null);

public sealed class GetActivosPorResponsableQueryHandler
    : IQueryHandler<GetActivosPorResponsableQuery, IReadOnlyList<ActivosPorResponsableDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetActivosPorResponsableQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ActivosPorResponsableDto>> HandleAsync(
        GetActivosPorResponsableQuery query,
        CancellationToken cancellationToken = default)
    {
        var idEmpresa = ActivoReporteConsulta.EmpresaEfectiva(_currentUser, query.IdEmpresa);
        var filas = await ActivoReporteConsulta.CargarAsync(_db, idEmpresa, cancellationToken);

        var asignados = filas
            .Where(f => f.EstadoOperativo == ActivoEstadoOperativo.Asignado && f.IdResponsable.HasValue)
            .ToList();

        var ids = asignados.Select(f => f.IdResponsable!.Value).Distinct().ToList();
        var nombres = await _db.Responsables
            .AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.NombreCompleto, cancellationToken);

        return asignados
            .GroupBy(f => f.IdResponsable!.Value)
            .OrderBy(g => nombres.GetValueOrDefault(g.Key, string.Empty))
            .Select(g => new ActivosPorResponsableDto(
                g.Key,
                nombres.GetValueOrDefault(g.Key, string.Empty),
                g.Count()))
            .ToList();
    }
}
