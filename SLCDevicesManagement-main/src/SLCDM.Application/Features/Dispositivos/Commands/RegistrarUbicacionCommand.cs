using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Dispositivos.Commands;

/// <summary>
/// IdActivo llega desde la claim del DeviceToken (Api/Authentication), nunca
/// del cuerpo de la peticion: un dispositivo solo puede reportar su propia
/// ubicacion, jamas la de otro activo.
/// </summary>
public sealed record RegistrarUbicacionCommand(
    int IdActivo,
    string? Bssid,
    decimal? Latitud,
    decimal? Longitud);

public sealed class RegistrarUbicacionCommandHandler : ICommandHandler<RegistrarUbicacionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly DeviceTrackingOptions _options;

    public RegistrarUbicacionCommandHandler(IApplicationDbContext db, IOptions<DeviceTrackingOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task HandleAsync(RegistrarUbicacionCommand command, CancellationToken cancellationToken = default)
    {
        var bssid = BssidFormat.Normalize(command.Bssid);

        var activo = await _db.Activos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == command.IdActivo, cancellationToken)
            ?? throw new NotFoundException("Activo", command.IdActivo);

        var token = await _db.DispositivosToken.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.IdActivo == command.IdActivo && !d.Revocado, cancellationToken)
            ?? throw new NotFoundException("DispositivoToken", command.IdActivo);

        var redConocida = bssid is null
            ? null
            : (await _db.RedesConocidas.IgnoreQueryFilters()
                .Include(r => r.Ubicacion)
                .ToListAsync(cancellationToken))
                .FirstOrDefault(r => BssidFormat.Normalize(r.Bssid) == bssid);

        var ubicacionDetectada = redConocida?.Ubicacion;
        var gpsOk = GeoCoords.EsUtilizable(command.Latitud, command.Longitud);
        var wifiCoordsOk = ubicacionDetectada is not null
            && GeoCoords.EsUtilizable(ubicacionDetectada.Latitud, ubicacionDetectada.Longitud);

        token.UltimoBssid = bssid;
        token.UltimoUsoEn = DateTime.UtcNow;
        token.UltimaUbicacionDetectadaId = ubicacionDetectada?.Id;

        if (gpsOk)
        {
            token.UltimaLatitud = command.Latitud;
            token.UltimaLongitud = command.Longitud;
            token.OrigenCoordenada = "gps";
        }
        else if (wifiCoordsOk)
        {
            token.UltimaLatitud = ubicacionDetectada!.Latitud;
            token.UltimaLongitud = ubicacionDetectada.Longitud;
            token.OrigenCoordenada = "wifi";
        }
        else
        {
            token.UltimaLatitud = null;
            token.UltimaLongitud = null;
            token.OrigenCoordenada = null;
        }

        if (activo.IdUbicacion == 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var ubicacionAsignada = await _db.Ubicaciones.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == activo.IdUbicacion, cancellationToken);

        var estaFueraDeRango = gpsOk
            ? EstaFueraDeGeocerca(command.Latitud!.Value, command.Longitud!.Value, ubicacionAsignada)
            : ubicacionDetectada is null || ubicacionDetectada.Id != activo.IdUbicacion;
        var eraFueraDeRango = token.FueraDeRango;

        token.FueraDeRango = estaFueraDeRango;
        await _db.SaveChangesAsync(cancellationToken);

        if (estaFueraDeRango == eraFueraDeRango)
        {
            return;
        }

        _db.HistorialActivos.Add(new HistorialActivo
        {
            FechaHora = DateTime.UtcNow,
            TipoOperacion = estaFueraDeRango ? "AlertaFueraDeRango" : "AlertaResuelta",
            Descripcion = estaFueraDeRango
                ? "El activo fue detectado fuera de la ubicacion asignada"
                : "El activo volvio a la ubicacion asignada",
            InformacionAnterior = $"id_ubicacion_asignada={activo.IdUbicacion}",
            InformacionNueva = gpsOk
                ? $"origen=gps; lat={command.Latitud}; lng={command.Longitud}; bssid={bssid ?? "ninguno"}"
                : ubicacionDetectada is not null
                    ? $"origen=wifi; id_ubicacion_detectada={ubicacionDetectada.Id}; bssid={bssid}"
                    : $"origen=wifi; ubicacion_detectada=desconocida; bssid={bssid}"
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private bool EstaFueraDeGeocerca(decimal latitud, decimal longitud, Ubicacion? ubicacionAsignada)
    {
        if (ubicacionAsignada is null)
        {
            return false;
        }

        if (!GeoCoords.EsUtilizable(ubicacionAsignada.Latitud, ubicacionAsignada.Longitud))
        {
            return true;
        }

        var radio = _options.GeofenceRadiusMeters > 0 ? _options.GeofenceRadiusMeters : 250;
        return GeoDistance.Meters(latitud, longitud, ubicacionAsignada.Latitud, ubicacionAsignada.Longitud) > radio;
    }
}
