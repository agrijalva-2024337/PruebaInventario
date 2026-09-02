using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Application.Features.Asignaciones;
using SLCDM.Application.Features.HistoricosInventario;

namespace SLCDM.Application.Features.HistoricosInventario.Queries;

public sealed record GetDiferenciasInventarioQuery(int IdHistoricoInventario, bool IncluirAbierta = false);

public sealed record DiferenciaActivoDto(
    int IdActivo,
    string NombreActivo,
    int IdUbicacionTeorica,
    string NombreUbicacion,
    bool? Encontrado,
    bool? BuenEstado,
    string? Observaciones);

public sealed record DiferenciasInventarioDto(
    int IdHistoricoInventario,
    int IdSede,
    bool Cerrado,
    IReadOnlyList<DiferenciaActivoDto> EncontradosEnSede,
    IReadOnlyList<DiferenciaActivoDto> NoEncontrados,
    IReadOnlyList<DiferenciaActivoDto> Sobrantes,
    IReadOnlyList<DiferenciaActivoDto> FaltantesSinRegistrar,
    IReadOnlyList<DiferenciaActivoDto> EncontradosEnOtraUbicacion);

public sealed class GetDiferenciasInventarioQueryValidator : AbstractValidator<GetDiferenciasInventarioQuery>
{
    public GetDiferenciasInventarioQueryValidator()
    {
        RuleFor(x => x.IdHistoricoInventario).RequiredId("id historico inventario");
    }
}

public sealed class GetDiferenciasInventarioQueryHandler
    : IQueryHandler<GetDiferenciasInventarioQuery, DiferenciasInventarioDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetDiferenciasInventarioQuery> _validator;

    public GetDiferenciasInventarioQueryHandler(
        IApplicationDbContext db,
        IValidator<GetDiferenciasInventarioQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<DiferenciasInventarioDto> HandleAsync(
        GetDiferenciasInventarioQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var jornada = await _db.HistoricosInventario
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == query.IdHistoricoInventario, cancellationToken)
            ?? throw new NotFoundException("HistoricoInventario", query.IdHistoricoInventario);

        if (!jornada.Cerrado && !query.IncluirAbierta)
        {
            throw new ConflictException(
                "La jornada de inventario no esta cerrada. El comparativo se genera al cierre.");
        }

        var teoricos = await (
                from a in _db.Activos.AsNoTracking()
                join u in _db.Ubicaciones.AsNoTracking() on a.IdUbicacion equals u.Id
                where u.IdSede == jornada.IdSede
                select new
                {
                    a.Id,
                    a.Nombre,
                    a.IdUbicacion,
                    NombreUbicacion = u.Nombre
                })
            .ToListAsync(cancellationToken);

        var idsBaja = await ActivoBajaRules.IdsDadosDeBajaAsync(_db, cancellationToken);
        teoricos = teoricos.Where(t => !idsBaja.Contains(t.Id)).ToList();

        var idUbicacionFiltro = InventarioJornadaRules.ParseIdUbicacionDelimite(jornada.Observaciones);
        if (idUbicacionFiltro.HasValue)
        {
            teoricos = teoricos.Where(t => t.IdUbicacion == idUbicacionFiltro.Value).ToList();
        }

        var hallazgos = await (
                from d in _db.DetallesActivos.AsNoTracking()
                join a in _db.Activos.AsNoTracking() on d.IdActivo equals a.Id
                join u in _db.Ubicaciones.AsNoTracking() on a.IdUbicacion equals u.Id
                where d.IdHistoricoInventario == jornada.Id
                select new
                {
                    d.IdActivo,
                    NombreActivo = a.Nombre,
                    a.IdUbicacion,
                    NombreUbicacion = u.Nombre,
                    u.IdSede,
                    d.Encontrado,
                    d.BuenEstado,
                    d.Observaciones
                })
            .ToListAsync(cancellationToken);

        var idsConHallazgo = hallazgos.Select(h => h.IdActivo).ToHashSet();

        var encontrados = hallazgos
            .Where(h => h.IdSede == jornada.IdSede && h.Encontrado)
            .Select(h => ToDto(h.IdActivo, h.NombreActivo, h.IdUbicacion, h.NombreUbicacion, h.Encontrado, h.BuenEstado, h.Observaciones))
            .ToList();

        var noEncontrados = hallazgos
            .Where(h => h.IdSede == jornada.IdSede && !h.Encontrado)
            .Select(h => ToDto(h.IdActivo, h.NombreActivo, h.IdUbicacion, h.NombreUbicacion, h.Encontrado, h.BuenEstado, h.Observaciones))
            .ToList();

        var sobrantes = hallazgos
            .Where(h => h.IdSede != jornada.IdSede)
            .Select(h => ToDto(h.IdActivo, h.NombreActivo, h.IdUbicacion, h.NombreUbicacion, h.Encontrado, h.BuenEstado, h.Observaciones))
            .ToList();

        var faltantes = teoricos
            .Where(t => !idsConHallazgo.Contains(t.Id))
            .Select(t => ToDto(t.Id, t.Nombre, t.IdUbicacion, t.NombreUbicacion, null, null, null))
            .ToList();

        var otraUbicacion = hallazgos
            .Where(h =>
                InventarioJornadaRules.EsOtraUbicacion(h.Observaciones)
                || h.IdSede != jornada.IdSede
                || (idUbicacionFiltro.HasValue && h.IdUbicacion != idUbicacionFiltro.Value))
            .Select(h => ToDto(h.IdActivo, h.NombreActivo, h.IdUbicacion, h.NombreUbicacion, h.Encontrado, h.BuenEstado, h.Observaciones))
            .ToList();

        return new DiferenciasInventarioDto(
            jornada.Id,
            jornada.IdSede,
            jornada.Cerrado,
            encontrados,
            noEncontrados,
            sobrantes,
            faltantes,
            otraUbicacion);
    }

    private static DiferenciaActivoDto ToDto(
        int idActivo,
        string nombreActivo,
        int idUbicacion,
        string nombreUbicacion,
        bool? encontrado,
        bool? buenEstado,
        string? observaciones) =>
        new(idActivo, nombreActivo, idUbicacion, nombreUbicacion, encontrado, buenEstado, observaciones);
}
