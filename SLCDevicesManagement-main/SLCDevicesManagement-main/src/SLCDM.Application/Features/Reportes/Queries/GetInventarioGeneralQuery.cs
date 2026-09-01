using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Reportes.Queries;

public sealed record GetInventarioGeneralQuery(int? IdEmpresa = null);

public sealed class GetInventarioGeneralQueryHandler
    : IQueryHandler<GetInventarioGeneralQuery, IReadOnlyList<InventarioEmpresaResumenDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetInventarioGeneralQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InventarioEmpresaResumenDto>> HandleAsync(
        GetInventarioGeneralQuery query,
        CancellationToken cancellationToken = default)
    {
        var idEmpresa = ActivoReporteConsulta.EmpresaEfectiva(_currentUser, query.IdEmpresa);
        var filas = await ActivoReporteConsulta.CargarAsync(_db, idEmpresa, cancellationToken);

        return filas
            .GroupBy(f => new { f.IdEmpresa, f.NombreEmpresa })
            .OrderBy(g => g.Key.NombreEmpresa)
            .Select(g => Contar(g.Key.IdEmpresa, g.Key.NombreEmpresa, g))
            .ToList();
    }

    private static InventarioEmpresaResumenDto Contar(
        int idEmpresa,
        string nombre,
        IEnumerable<ActivoInventarioRow> filas)
    {
        var list = filas.ToList();
        return new InventarioEmpresaResumenDto(
            idEmpresa,
            nombre,
            list.Count,
            list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Disponible),
            list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Asignado),
            list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Mantenimiento),
            list.Count(f => f.EstadoOperativo == ActivoEstadoOperativo.Baja),
            list.Sum(f => f.Activo.CostoAdquisicion));
    }
}
