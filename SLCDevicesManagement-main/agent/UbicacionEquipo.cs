using System.Diagnostics;
using System.Globalization;

namespace SLCDM.Agent;

/// <summary>
/// Windows Location. Rechaza 0,0 (oceano) y precision peor de 5 km.
/// En LocalSystem suele devolver vacio; entonces el backend usa el BSSID.
/// </summary>
public static class UbicacionEquipo
{
    public static (decimal Latitud, decimal Longitud)? Leer()
    {
        const string script =
            "Add-Type -AssemblyName System.Device; " +
            "$w = New-Object System.Device.Location.GeoCoordinateWatcher([System.Device.Location.GeoPositionAccuracy]::High); " +
            "$w.Start(); $n = 0; " +
            "while ($w.Status -ne 'Ready' -and $w.Status -ne 'Initializing' -and $n -lt 40) { Start-Sleep -Milliseconds 250; $n++ }; " +
            "while ($w.Status -eq 'Initializing' -and $n -lt 40) { Start-Sleep -Milliseconds 250; $n++ }; " +
            "$c = $w.Position.Location; $w.Stop(); " +
            "if ($null -eq $c -or $c.IsUnknown) { exit 1 }; " +
            "if ([double]::IsNaN($c.Latitude) -or [double]::IsNaN($c.Longitude)) { exit 1 }; " +
            "if ([math]::Abs($c.Latitude) -lt 0.05 -and [math]::Abs($c.Longitude) -lt 0.05) { exit 1 }; " +
            "if (-not [double]::IsNaN($c.HorizontalAccuracy) -and $c.HorizontalAccuracy -gt 5000) { exit 1 }; " +
            "$inv = [cultureinfo]::InvariantCulture; " +
            "Write-Output ($c.Latitude.ToString($inv) + '|' + $c.Longitude.ToString($inv))";

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

            if (!proceso.WaitForExit(12_000))
            {
                proceso.Kill(entireProcessTree: true);
                return null;
            }

            if (proceso.ExitCode != 0)
            {
                return null;
            }

            var linea = proceso.StandardOutput.ReadToEnd().Trim().Replace(',', '.');
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

            if (Math.Abs((double)lat) < 0.05 && Math.Abs((double)lng) < 0.05)
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
