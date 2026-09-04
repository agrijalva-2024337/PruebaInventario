using System.Management;
using System.Runtime.Versioning;

namespace SLCDM.Agent;

[SupportedOSPlatform("windows")]

public static class HuellaHardware
{
    public static string? LeerNumeroSerieBios()
    {
        using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
        foreach (ManagementObject item in searcher.Get())
        {
            return item["SerialNumber"]?.ToString()?.Trim();
        }
        return null;
    }

    public static string? LeerNumeroSerieMotherboard()
    {
        using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
        foreach (ManagementObject item in searcher.Get())
        {
            return item["SerialNumber"]?.ToString()?.Trim();
        }
        return null;
    }
}
