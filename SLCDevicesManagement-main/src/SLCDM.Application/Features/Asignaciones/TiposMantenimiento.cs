namespace SLCDM.Application.Features.Asignaciones;

/// <summary>
/// Preventivo/correctivo via texto en observaciones (sin tabla extra en el ERD).
/// </summary>
public static class TiposMantenimiento
{
    public const string Preventivo = "Preventivo";
    public const string Correctivo = "Correctivo";

    public static string Prefijo(string? tipo)
    {
        var n = TipoAsignacionNombres.Normalizar(tipo);
        if (n.Equals(TipoAsignacionNombres.Normalizar(Correctivo), StringComparison.OrdinalIgnoreCase))
        {
            return $"[{Correctivo}] ";
        }

        return $"[{Preventivo}] ";
    }
}
