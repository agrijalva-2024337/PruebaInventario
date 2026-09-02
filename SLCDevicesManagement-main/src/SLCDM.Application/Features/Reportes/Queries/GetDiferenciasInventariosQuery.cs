using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Features.HistoricosInventario.Queries;

namespace SLCDM.Application.Features.Reportes.Queries;

public sealed record GetDiferenciasInventariosQuery(int? IdEmpresa = null);

public sealed record DiferenciaInventarioReporteDto(
    int IdHistoricoInventario,
    string NombreSede,
    DateTime FechaInicio,
    string Tipo,
    int IdActivo,
    string NombreActivo,
    string NombreUbicacion,
    string? Observaciones);

public sealed class GetDiferenciasInventariosQueryHandler
    : IQueryHandler<GetDiferenciasInventariosQuery, IReadOnlyList<DiferenciaInventarioReporteDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IQueryHandler<GetDiferenciasInventarioQuery, DiferenciasInventarioDto> _diferencias;

    public GetDiferenciasInventariosQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IQueryHandler<GetDiferenciasInventarioQuery, DiferenciasInventarioDto> diferencias)
    {
        _db = db;
        _currentUser = currentUser;
        _diferencias = diferencias;
    }

    public async Task<IReadOnlyList<DiferenciaInventarioReporteDto>> HandleAsync(
        GetDiferenciasInventariosQuery query,
        CancellationToken cancellationToken = default)
    {
        var idEmpresa = ActivoReporteConsulta.EmpresaEfectiva(_currentUser, query.IdEmpresa);

        var jornadasQuery =
            from h in _db.HistoricosInventario.AsNoTracking()
            join s in _db.Sedes.AsNoTracking() on h.IdSede equals s.Id
            where h.Cerrado
            select new { h.Id, h.FechaInicio, NombreSede = s.Nombre, s.IdEmpresa };

        if (idEmpresa.HasValue)
        {
            jornadasQuery = jornadasQuery.Where(x => x.IdEmpresa == idEmpresa.Value);
        }

        var jornadas = await jornadasQuery
            .OrderByDescending(x => x.FechaInicio)
            .ToListAsync(cancellationToken);

        var resultado = new List<DiferenciaInventarioReporteDto>();
        foreach (var jornada in jornadas)
        {
            var dif = await _diferencias.HandleAsync(
                new GetDiferenciasInventarioQuery(jornada.Id),
                cancellationToken);

            Agregar(resultado, jornada.Id, jornada.NombreSede, jornada.FechaInicio, "No encontrado", dif.NoEncontrados);
            Agregar(resultado, jornada.Id, jornada.NombreSede, jornada.FechaInicio, "Sobrante", dif.Sobrantes);
            Agregar(resultado, jornada.Id, jornada.NombreSede, jornada.FechaInicio, "Faltante", dif.FaltantesSinRegistrar);
            Agregar(resultado, jornada.Id, jornada.NombreSede, jornada.FechaInicio, "Otra ubicacion", dif.EncontradosEnOtraUbicacion);
        }

        return resultado;
    }

    private static void Agregar(
        List<DiferenciaInventarioReporteDto> destino,
        int idJornada,
        string nombreSede,
        DateTime fechaInicio,
        string tipo,
        IReadOnlyList<DiferenciaActivoDto> items)
    {
        foreach (var item in items)
        {
            destino.Add(new DiferenciaInventarioReporteDto(
                idJornada,
                nombreSede,
                fechaInicio,
                tipo,
                item.IdActivo,
                item.NombreActivo,
                item.NombreUbicacion,
                item.Observaciones));
        }
    }
}
