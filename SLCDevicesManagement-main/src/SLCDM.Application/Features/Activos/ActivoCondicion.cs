namespace SLCDM.Application.Features.Activos;

public static class ActivoCondicion
{
    public const string Nuevo = "Nuevo";
    public const string Bueno = "Bueno";
    public const string Regular = "Regular";
    public const string Malo = "Malo";

    public static readonly string[] Todas = [Nuevo, Bueno, Regular, Malo];

    public static bool EsValida(string? value) =>
        Todas.Any(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string Texto(string? value)
    {
        var trimmed = value?.Trim();
        var match = Todas.FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));
        return match ?? (string.IsNullOrWhiteSpace(trimmed) ? "—" : trimmed);
    }
}
