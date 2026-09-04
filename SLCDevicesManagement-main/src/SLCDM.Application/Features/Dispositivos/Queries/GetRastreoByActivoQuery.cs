using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Dispositivos.Queries;

public sealed record GetRastreoByActivoQuery(int IdActivo);

public sealed class GetRastreoByActivoQueryHandler
    : IQueryHandler<GetRastreoByActivoQuery, DispositivoRastreoDto>
{
    private readonly IApplicationDbContext _db;

    public GetRastreoByActivoQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DispositivoRastreoDto> HandleAsync(
        GetRastreoByActivoQuery query,
        CancellationToken cancellationToken = default)
    {
        var dto = await _db.DispositivosToken.AsNoTracking()
            .Where(d => !d.Revocado && d.IdActivo == query.IdActivo)
            .Select(d => new DispositivoRastreoDto(
                d.IdActivo,
                d.Activo!.Nombre,
                d.FueraDeRango,
                d.UltimoUsoEn,
                d.UltimoBssid,
                d.OrigenCoordenada,
                d.UltimaLatitud,
                d.UltimaLongitud,
                d.Activo.Ubicacion == null
                    ? null
                    : new UbicacionMapaDto(
                        d.Activo.Ubicacion.Id,
                        d.Activo.Ubicacion.Nombre,
                        d.Activo.Ubicacion.Latitud,
                        d.Activo.Ubicacion.Longitud),
                d.UltimaUbicacionDetectada == null
                    ? null
                    : new UbicacionMapaDto(
                        d.UltimaUbicacionDetectada.Id,
                        d.UltimaUbicacionDetectada.Nombre,
                        d.UltimaUbicacionDetectada.Latitud,
                        d.UltimaUbicacionDetectada.Longitud)))
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException("DispositivoToken", query.IdActivo);
    }
}
