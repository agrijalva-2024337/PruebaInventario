namespace SLCDM.Application.Common;

public static class GeoCoords
{
    public static bool EsUtilizable(decimal? latitud, decimal? longitud)
    {
        if (latitud is null || longitud is null)
        {
            return false;
        }

        if (latitud is < -90 or > 90 || longitud is < -180 or > 180)
        {
            return false;
        }

        // GeoCoordinateWatcher y formularios vacios suelen mandar 0,0 (Golfo de Guinea).
        return Math.Abs((double)latitud.Value) >= 0.05 || Math.Abs((double)longitud.Value) >= 0.05;
    }
}
