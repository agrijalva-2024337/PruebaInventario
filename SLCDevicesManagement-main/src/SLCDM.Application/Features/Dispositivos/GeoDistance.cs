namespace SLCDM.Application.Features.Dispositivos;

public static class GeoDistance
{
    public static double Meters(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double earthRadiusMeters = 6_371_000;
        var phi1 = DegreesToRadians((double)lat1);
        var phi2 = DegreesToRadians((double)lat2);
        var dPhi = DegreesToRadians((double)(lat2 - lat1));
        var dLambda = DegreesToRadians((double)(lon2 - lon1));

        var a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2)
            + Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
