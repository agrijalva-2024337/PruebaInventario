namespace SLCDM.Application.Common.Options;

public sealed class DeviceTrackingOptions
{
    public const string SectionName = "DeviceTracking";

    public string Pepper { get; set; } = string.Empty;

    public string InstallKey { get; set; } = string.Empty;

    public int TokenExpiryDays { get; set; } = 365;

    /// <summary>
    /// Radio alrededor de la ubicacion asignada. Solo se usa cuando el
    /// agente manda coordenadas propias del equipo (GPS / Windows Location).
    /// </summary>
    public int GeofenceRadiusMeters { get; set; } = 250;
}
