using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Dispositivos.Queries;

public sealed record GetDispositivosFueraDeRangoQuery;

public sealed record DispositivoFueraDeRangoDto(
    int IdActivo,
    string NombreActivo,
    int? IdUbicacionAsignada,
    int? IdUbicacionDetectada,
    DateTime? UltimoUsoEn,
    string? UltimoBssid,
    string? OrigenCoordenada,
    decimal? UltimaLatitud,
    decimal? UltimaLongitud);

public sealed class GetDispositivosFueraDeRangoQueryHandler
    : IQueryHandler<GetDispositivosFueraDeRangoQuery, IReadOnlyList<DispositivoFueraDeRangoDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDispositivosFueraDeRangoQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DispositivoFueraDeRangoDto>> HandleAsync(
        GetDispositivosFueraDeRangoQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = await _db.DispositivosToken.AsNoTracking()
            .Where(d => d.FueraDeRango && !d.Revocado)
            .Include(d => d.Activo)
            .Select(d => new DispositivoFueraDeRangoDto(
                d.IdActivo,
                d.Activo!.Nombre,
                d.Activo.IdUbicacion,
                d.UltimaUbicacionDetectadaId,
                d.UltimoUsoEn,
                d.UltimoBssid,
                d.OrigenCoordenada,
                d.UltimaLatitud,
                d.UltimaLongitud))
            .ToListAsync(cancellationToken);

        return items;
    }
}
