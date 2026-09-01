using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Reportes.Queries;

public sealed record GetActivosPorCategoriaQuery(int? IdEmpresa = null);

public sealed class GetActivosPorCategoriaQueryHandler
    : IQueryHandler<GetActivosPorCategoriaQuery, IReadOnlyList<ActivosPorCategoriaDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetActivosPorCategoriaQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ActivosPorCategoriaDto>> HandleAsync(
        GetActivosPorCategoriaQuery query,
        CancellationToken cancellationToken = default)
    {
        var idEmpresa = ActivoReporteConsulta.EmpresaEfectiva(_currentUser, query.IdEmpresa);
        var filas = await ActivoReporteConsulta.CargarAsync(_db, idEmpresa, cancellationToken);

        return filas
            .GroupBy(f => new { f.Activo.IdCategoriaActivo, f.NombreCategoria })
            .OrderBy(g => g.Key.NombreCategoria)
            .Select(g =>
            {
                var list = g.ToList();
                return new ActivosPorCategoriaDto(
                    g.Key.IdCategoriaActivo,
                    g.Key.NombreCategoria,
                    list.Count,
                    list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Disponible),
                    list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Asignado),
                    list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Mantenimiento),
                    list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Baja));
            })
            .ToList();
    }
}
