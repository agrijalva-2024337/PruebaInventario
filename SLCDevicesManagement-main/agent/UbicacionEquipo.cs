using System.Diagnostics;
using System.Globalization;

namespace SLCDM.Agent;

/// <summary>
/// Intenta leer la posicion del equipo via Windows Location
/// (GPS o triangulacion Wi-Fi). En un servicio LocalSystem suele fallar
/// si la ubicacion del SO esta desactivada; en ese caso el backend usa
/// la coordenada de la red Wi-Fi catalogada.
/// </summary>
public static class UbicacionEquipo
{
    public static (decimal Latitud, decimal Longitud)? Leer()
    {
        const string script =
            "Add-Type -AssemblyName System.Device; " +
            "$w = New-Object System.Device.Location.GeoCoordinateWatcher; " +
            "$w.Start(); $n = 0; " +
            "while ($w.Status -ne 'Ready' -and $n -lt 32) { Start-Sleep -Milliseconds 250; $n++ }; " +
            "$c = $w.Position.Location; $w.Stop(); " +
            "if ($c.IsUnknown) { exit 1 }; " +
            "Write-Output ('{0}|{1}' -f $c.Latitude, $c.Longitude)";

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"" + script + "\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var proceso = Process.Start(psi);
            if (proceso is null)
            {
                return null;
            }

            if (!proceso.WaitForExit(10_000))
            {
                proceso.Kill(entireProcessTree: true);
                return null;
            }

            if (proceso.ExitCode != 0)
            {
                return null;
            }

            var linea = proceso.StandardOutput.ReadToEnd().Trim();
            var partes = linea.Split('|');
            if (partes.Length != 2)
            {
                return null;
            }

            if (!decimal.TryParse(partes[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
                || !decimal.TryParse(partes[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
            {
                return null;
            }

            if (lat is < -90 or > 90 || lng is < -180 or > 180)
            {
                return null;
            }

            return (lat, lng);
        }
        catch
        {
            return null;
        }
    }
}
