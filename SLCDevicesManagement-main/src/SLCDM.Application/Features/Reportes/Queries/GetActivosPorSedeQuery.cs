using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Reportes.Queries;

public sealed record GetActivosPorSedeQuery(int? IdEmpresa = null, int? IdSede = null);

public sealed class GetActivosPorSedeQueryHandler
    : IQueryHandler<GetActivosPorSedeQuery, IReadOnlyList<ActivosPorSedeDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetActivosPorSedeQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ActivosPorSedeDto>> HandleAsync(
        GetActivosPorSedeQuery query,
        CancellationToken cancellationToken = default)
    {
        var idEmpresa = ActivoReporteConsulta.EmpresaEfectiva(_currentUser, query.IdEmpresa);
        var filas = await ActivoReporteConsulta.CargarAsync(_db, idEmpresa, cancellationToken);

        if (query.IdSede is > 0)
        {
            filas = filas.Where(f => f.IdSede == query.IdSede.Value).ToList();
        }

        return filas
            .GroupBy(f => new { f.IdSede, f.NombreSede, f.IdEmpresa })
            .OrderBy(g => g.Key.NombreSede)
            .Select(g =>
            {
                var list = g.ToList();
                return new ActivosPorSedeDto(
                    g.Key.IdSede,
                    g.Key.NombreSede,
                    g.Key.IdEmpresa,
                    list.Count,
                    list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Disponible),
                    list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Asignado),
                    list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Mantenimiento),
                    list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Baja));
            })
            .ToList();
    }
}
